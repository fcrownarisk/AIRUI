#!/usr/bin/env python3
"""
Minecraft Clone with Abundant Coordinate Display
Shows block coordinates, player position, chunk data, and more
Uses Pygame for rendering and NumPy for efficient world storage
"""

import pygame
import numpy as np
import math
import random
from pygame.locals import *
from OpenGL.GL import *
from OpenGL.GLU import *
from OpenGL.GL.shaders import *
import ctypes
import sys
import time

# ============================================================================
# Constants and Configuration
# ============================================================================

# Window settings
WINDOW_WIDTH = 1280
WINDOW_HEIGHT = 720
FPS = 60

# World settings
WORLD_SIZE_X = 32
WORLD_SIZE_Y = 32
WORLD_SIZE_Z = 32
CHUNK_SIZE = 16
RENDER_DISTANCE = 8  # chunks

# Block types
BLOCK_AIR = 0
BLOCK_STONE = 1
BLOCK_DIRT = 2
BLOCK_GRASS = 3
BLOCK_WOOD = 4
BLOCK_LEAVES = 5
BLOCK_SAND = 6
BLOCK_WATER = 7
BLOCK_BEDROCK = 8
BLOCK_COAL_ORE = 9
BLOCK_IRON_ORE = 10
BLOCK_GOLD_ORE = 11
BLOCK_DIAMOND_ORE = 12
BLOCK_OBSIDIAN = 13

# Block colors (RGB)
BLOCK_COLORS = {
    BLOCK_AIR: (0, 0, 0, 0),
    BLOCK_STONE: (100, 100, 100),
    BLOCK_DIRT: (139, 69, 19),
    BLOCK_GRASS: (34, 139, 34),
    BLOCK_WOOD: (160, 82, 45),
    BLOCK_LEAVES: (0, 100, 0),
    BLOCK_SAND: (238, 203, 173),
    BLOCK_WATER: (64, 164, 223),
    BLOCK_BEDROCK: (50, 50, 50),
    BLOCK_COAL_ORE: (50, 50, 50),
    BLOCK_IRON_ORE: (180, 130, 70),
    BLOCK_GOLD_ORE: (255, 215, 0),
    BLOCK_DIAMOND_ORE: (185, 242, 255),
    BLOCK_OBSIDIAN: (50, 30, 50),
}

# Block names for display
BLOCK_NAMES = {
    BLOCK_AIR: "Air",
    BLOCK_STONE: "Stone",
    BLOCK_DIRT: "Dirt",
    BLOCK_GRASS: "Grass",
    BLOCK_WOOD: "Wood",
    BLOCK_LEAVES: "Leaves",
    BLOCK_SAND: "Sand",
    BLOCK_WATER: "Water",
    BLOCK_BEDROCK: "Bedrock",
    BLOCK_COAL_ORE: "Coal Ore",
    BLOCK_IRON_ORE: "Iron Ore",
    BLOCK_GOLD_ORE: "Gold Ore",
    BLOCK_DIAMOND_ORE: "Diamond Ore",
    BLOCK_OBSIDIAN: "Obsidian",
}

# Movement speeds
MOVEMENT_SPEED = 5.0
MOUSE_SENSITIVITY = 0.1
JUMP_FORCE = 8.0
GRAVITY = 20.0

# ============================================================================
# Chunk Class - Manages a 16x16x16 section of the world
# ============================================================================

class Chunk:
    """Represents a 16x16x16 chunk of the world with block data"""
    
    def __init__(self, cx, cy, cz):
        self.cx = cx
        self.cy = cy
        self.cz = cz
        self.blocks = np.zeros((CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE), dtype=np.uint8)
        self.dirty = True
        self.vbo = None
        self.vertex_count = 0
        self.generated = False
        
    def get_block(self, x, y, z):
        """Get block at local coordinates"""
        if 0 <= x < CHUNK_SIZE and 0 <= y < CHUNK_SIZE and 0 <= z < CHUNK_SIZE:
            return self.blocks[x, y, z]
        return BLOCK_AIR
    
    def set_block(self, x, y, z, block_type):
        """Set block at local coordinates"""
        if 0 <= x < CHUNK_SIZE and 0 <= y < CHUNK_SIZE and 0 <= z < CHUNK_SIZE:
            self.blocks[x, y, z] = block_type
            self.dirty = True
            
    def generate_terrain(self, height_map, cave_map):
        """Generate terrain for this chunk using height maps"""
        for lx in range(CHUNK_SIZE):
            for lz in range(CHUNK_SIZE):
                wx = self.cx * CHUNK_SIZE + lx
                wz = self.cz * CHUNK_SIZE + lz
                
                # Get height from height map
                height = height_map[wx, wz]
                
                for ly in range(CHUNK_SIZE):
                    wy = self.cy * CHUNK_SIZE + ly
                    
                    # Determine block type based on height and caves
                    if wy > height:
                        self.blocks[lx, ly, lz] = BLOCK_AIR
                    elif wy == height:
                        self.blocks[lx, ly, lz] = BLOCK_GRASS
                    elif wy > height - 4:
                        self.blocks[lx, ly, lz] = BLOCK_DIRT
                    elif wy > 0:
                        # Stone layer with ores
                        if cave_map[wx, wy, wz] > 0.8:
                            self.blocks[lx, ly, lz] = BLOCK_AIR  # Cave
                        else:
                            # Random ore distribution
                            rand_val = random.random()
                            if wy < 16 and rand_val < 0.01:
                                self.blocks[lx, ly, lz] = BLOCK_DIAMOND_ORE
                            elif wy < 32 and rand_val < 0.02:
                                self.blocks[lx, ly, lz] = BLOCK_GOLD_ORE
                            elif wy < 48 and rand_val < 0.05:
                                self.blocks[lx, ly, lz] = BLOCK_IRON_ORE
                            elif wy < 64 and rand_val < 0.1:
                                self.blocks[lx, ly, lz] = BLOCK_COAL_ORE
                            else:
                                self.blocks[lx, ly, lz] = BLOCK_STONE
                    else:
                        self.blocks[lx, ly, lz] = BLOCK_BEDROCK
        
        self.generated = True
        self.dirty = True

# ============================================================================
# World Class - Manages all chunks and world generation
# ============================================================================

class World:
    """Main world container with chunk management"""
    
    def __init__(self):
        self.chunks = {}
        self.height_map = None
        self.cave_map = None
        self.generate_maps()
        
    def generate_maps(self):
        """Generate height and cave maps using Perlin-like noise"""
        world_width = WORLD_SIZE_X
        world_depth = WORLD_SIZE_Z
        
        # Simple height map using sine/cosine combinations
        self.height_map = np.zeros((world_width, world_depth), dtype=int)
        for x in range(world_width):
            for z in range(world_depth):
                # Multiple noise frequencies
                h1 = math.sin(x * 0.05) * math.cos(z * 0.05) * 15
                h2 = math.sin(x * 0.1) * math.cos(z * 0.1) * 8
                h3 = math.sin(x * 0.2) * math.cos(z * 0.2) * 4
                h4 = random.randint(-2, 2)
                
                height = 24 + int(h1 + h2 + h3 + h4)
                self.height_map[x, z] = max(1, min(WORLD_SIZE_Y - 2, height))
        
        # 3D cave map (simple random noise)
        self.cave_map = np.random.random((world_width, WORLD_SIZE_Y, world_depth))
        
    def get_chunk(self, cx, cy, cz):
        """Get chunk at chunk coordinates, create if doesn't exist"""
        key = (cx, cy, cz)
        if key not in self.chunks:
            self.chunks[key] = Chunk(cx, cy, cz)
            self.chunks[key].generate_terrain(self.height_map, self.cave_map)
        return self.chunks[key]
    
    def get_block(self, x, y, z):
        """Get block at world coordinates"""
        if x < 0 or x >= WORLD_SIZE_X or y < 0 or y >= WORLD_SIZE_Y or z < 0 or z >= WORLD_SIZE_Z:
            return BLOCK_AIR
        
        cx = x // CHUNK_SIZE
        cy = y // CHUNK_SIZE
        cz = z // CHUNK_SIZE
        lx = x % CHUNK_SIZE
        ly = y % CHUNK_SIZE
        lz = z % CHUNK_SIZE
        
        chunk = self.get_chunk(cx, cy, cz)
        return chunk.get_block(lx, ly, lz)
    
    def set_block(self, x, y, z, block_type):
        """Set block at world coordinates"""
        if x < 0 or x >= WORLD_SIZE_X or y < 0 or y >= WORLD_SIZE_Y or z < 0 or z >= WORLD_SIZE_Z:
            return
        
        cx = x // CHUNK_SIZE
        cy = y // CHUNK_SIZE
        cz = z // CHUNK_SIZE
        lx = x % CHUNK_SIZE
        ly = y % CHUNK_SIZE
        lz = z % CHUNK_SIZE
        
        chunk = self.get_chunk(cx, cy, cz)
        chunk.set_block(lx, ly, lz, block_type)

# ============================================================================
# Player Class - Handles movement and camera
# ============================================================================

class Player:
    """Player entity with position, rotation, and movement"""
    
    def __init__(self, world):
        self.world = world
        self.x = WORLD_SIZE_X / 2
        self.y = WORLD_SIZE_Y - 5
        self.z = WORLD_SIZE_Z / 2
        self.rot_x = 0  # Looking up/down
        self.rot_y = 0  # Looking left/right
        self.velocity_x = 0
        self.velocity_y = 0
        self.velocity_z = 0
        self.on_ground = False
        self.flying = False
        self.selected_block = None
        self.selected_face = None
        self.block_in_hand = BLOCK_STONE
        
    def update(self, dt, keys):
        """Update player position based on input and physics"""
        # Mouse look (handled by pygame)
        
        # Movement vectors
        forward_x = math.sin(math.radians(self.rot_y))
        forward_z = math.cos(math.radians(self.rot_y))
        right_x = math.sin(math.radians(self.rot_y + 90))
        right_z = math.cos(math.radians(self.rot_y + 90))
        
        # Apply movement based on keys
        move_speed = MOVEMENT_SPEED * dt
        if self.flying:
            move_speed *= 2
            
        if keys[K_w]:
            self.x += forward_x * move_speed
            self.z += forward_z * move_speed
        if keys[K_s]:
            self.x -= forward_x * move_speed
            self.z -= forward_z * move_speed
        if keys[K_a]:
            self.x -= right_x * move_speed
            self.z -= right_z * move_speed
        if keys[K_d]:
            self.x += right_x * move_speed
            self.z += right_z * move_speed
            
        # Vertical movement
        if self.flying:
            if keys[K_SPACE]:
                self.y += move_speed
            if keys[K_LSHIFT]:
                self.y -= move_speed
        else:
            # Gravity and jumping
            self.velocity_y -= GRAVITY * dt
            
            if keys[K_SPACE] and self.on_ground:
                self.velocity_y = JUMP_FORCE
                self.on_ground = False
                
            self.y += self.velocity_y * dt
            
            # Simple collision detection
            block_below = self.world.get_block(
                int(self.x), 
                int(self.y - 1.5), 
                int(self.z)
            )
            self.on_ground = block_below != BLOCK_AIR
            
            if self.on_ground and self.velocity_y < 0:
                self.velocity_y = 0
                self.y = int(self.y - 1.5) + 1.5
                
    def get_camera_position(self):
        """Get camera position for rendering"""
        return (self.x, self.y, self.z)
    
    def get_view_direction(self):
        """Get normalized view direction vector"""
        rad_y = math.radians(self.rot_y)
        rad_x = math.radians(self.rot_x)
        
        direction = [
            math.cos(rad_x) * math.sin(rad_y),
            -math.sin(rad_x),
            math.cos(rad_x) * math.cos(rad_y)
        ]
        return direction
    
    def raycast(self, max_distance=5):
        """Cast a ray from player to find targeted block"""
        direction = self.get_view_direction()
        
        # Start from just outside player
        start_x = self.x
        start_y = self.y - 1  # Eye height
        start_z = self.z
        
        # DDA algorithm for voxel traversal
        x = int(start_x)
        y = int(start_y)
        z = int(start_z)
        
        step_x = 1 if direction[0] > 0 else -1
        step_y = 1 if direction[1] > 0 else -1
        step_z = 1 if direction[2] > 0 else -1
        
        t_delta_x = abs(1.0 / direction[0]) if direction[0] != 0 else float('inf')
        t_delta_y = abs(1.0 / direction[1]) if direction[1] != 0 else float('inf')
        t_delta_z = abs(1.0 / direction[2]) if direction[2] != 0 else float('inf')
        
        # Initial t values
        if direction[0] > 0:
            t_max_x = (x + 1 - start_x) / direction[0]
        else:
            t_max_x = (start_x - x) / -direction[0]
            
        if direction[1] > 0:
            t_max_y = (y + 1 - start_y) / direction[1]
        else:
            t_max_y = (start_y - y) / -direction[1]
            
        if direction[2] > 0:
            t_max_z = (z + 1 - start_z) / direction[2]
        else:
            t_max_z = (start_z - z) / -direction[2]
        
        # Walk the ray
        for _ in range(max_distance * 2):
            if t_max_x < t_max_y and t_max_x < t_max_z:
                x += step_x
                t_max_x += t_delta_x
                face = 0 if step_x > 0 else 1
            elif t_max_y < t_max_z:
                y += step_y
                t_max_y += t_delta_y
                face = 2 if step_y > 0 else 3
            else:
                z += step_z
                t_max_z += t_delta_z
                face = 4 if step_z > 0 else 5
            
            # Check bounds
            if x < 0 or x >= WORLD_SIZE_X or y < 0 or y >= WORLD_SIZE_Y or z < 0 or z >= WORLD_SIZE_Z:
                break
                
            block = self.world.get_block(x, y, z)
            if block != BLOCK_AIR:
                self.selected_block = (x, y, z)
                self.selected_face = face
                return True
                
        self.selected_block = None
        self.selected_face = None
        return False

# ============================================================================
# Renderer Class - Handles all OpenGL rendering
# ============================================================================

class Renderer:
    """OpenGL renderer for the voxel world"""
    
    def __init__(self, world):
        self.world = world
        self.init_opengl()
        self.init_shaders()
        self.init_buffers()
        
    def init_opengl(self):
        """Initialize OpenGL settings"""
        glEnable(GL_DEPTH_TEST)
        glEnable(GL_CULL_FACE)
        glCullFace(GL_BACK)
        glClearColor(0.5, 0.7, 1.0, 1.0)
        
        # Set up projection matrix
        glMatrixMode(GL_PROJECTION)
        gluPerspective(70, WINDOW_WIDTH / WINDOW_HEIGHT, 0.1, 200.0)
        glMatrixMode(GL_MODELVIEW)
        
    def init_shaders(self):
        """Initialize basic shaders (fixed function for simplicity)"""
        # Using fixed function pipeline for simplicity
        pass
        
    def init_buffers(self):
        """Initialize VBOs for rendering"""
        # Will be created per chunk
        pass
        
    def render_world(self, player):
        """Render the entire world"""
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
        glLoadIdentity()
        
        # Set up camera
        cam_x, cam_y, cam_z = player.get_camera_position()
        gluLookAt(
            cam_x, cam_y, cam_z,
            cam_x + player.get_view_direction()[0],
            cam_y + player.get_view_direction()[1],
            cam_z + player.get_view_direction()[2],
            0, 1, 0
        )
        
        # Determine visible chunks
        cx = int(cam_x) // CHUNK_SIZE
        cy = int(cam_y) // CHUNK_SIZE
        cz = int(cam_z) // CHUNK_SIZE
        
        # Render chunks within render distance
        for dx in range(-RENDER_DISTANCE, RENDER_DISTANCE + 1):
            for dy in range(-2, 3):  # Limited vertical range
                for dz in range(-RENDER_DISTANCE, RENDER_DISTANCE + 1):
                    chunk_x = cx + dx
                    chunk_y = cy + dy
                    chunk_z = cz + dz
                    
                    if chunk_y < 0 or chunk_y >= WORLD_SIZE_Y // CHUNK_SIZE:
                        continue
                        
                    chunk = self.world.get_chunk(chunk_x, chunk_y, chunk_z)
                    self.render_chunk(chunk, chunk_x, chunk_y, chunk_z)
        
        # Render selection outline if player has selected a block
        if player.selected_block:
            self.render_selection(player.selected_block)
            
    def render_chunk(self, chunk, cx, cy, cz):
        """Render a single chunk"""
        # Simple rendering - draw each non-air block as a cube
        # In a real implementation, you'd use VBOs and face culling
        
        for x in range(CHUNK_SIZE):
            for y in range(CHUNK_SIZE):
                for z in range(CHUNK_SIZE):
                    block_type = chunk.blocks[x, y, z]
                    if block_type == BLOCK_AIR:
                        continue
                        
                    world_x = cx * CHUNK_SIZE + x
                    world_y = cy * CHUNK_SIZE + y
                    world_z = cz * CHUNK_SIZE + z
                    
                    self.render_block(world_x, world_y, world_z, block_type)
                    
    def render_block(self, x, y, z, block_type):
        """Render a single block as a colored cube"""
        color = BLOCK_COLORS.get(block_type, (200, 200, 200))
        
        glPushMatrix()
        glTranslatef(x + 0.5, y + 0.5, z + 0.5)
        
        # Draw cube with face colors
        glBegin(GL_QUADS)
        
        # Front face
        glColor3ub(*color)
        glVertex3f(-0.5, -0.5, 0.5)
        glVertex3f(0.5, -0.5, 0.5)
        glVertex3f(0.5, 0.5, 0.5)
        glVertex3f(-0.5, 0.5, 0.5)
        
        # Back face
        glColor3ub(*[int(c*0.8) for c in color])
        glVertex3f(-0.5, -0.5, -0.5)
        glVertex3f(-0.5, 0.5, -0.5)
        glVertex3f(0.5, 0.5, -0.5)
        glVertex3f(0.5, -0.5, -0.5)
        
        # Top face
        glColor3ub(*[int(c*1.2) for c in color])
        glVertex3f(-0.5, 0.5, -0.5)
        glVertex3f(-0.5, 0.5, 0.5)
        glVertex3f(0.5, 0.5, 0.5)
        glVertex3f(0.5, 0.5, -0.5)
        
        # Bottom face
        glColor3ub(*[int(c*0.6) for c in color])
        glVertex3f(-0.5, -0.5, -0.5)
        glVertex3f(0.5, -0.5, -0.5)
        glVertex3f(0.5, -0.5, 0.5)
        glVertex3f(-0.5, -0.5, 0.5)
        
        # Right face
        glColor3ub(*[int(c*0.9) for c in color])
        glVertex3f(0.5, -0.5, -0.5)
        glVertex3f(0.5, 0.5, -0.5)
        glVertex3f(0.5, 0.5, 0.5)
        glVertex3f(0.5, -0.5, 0.5)
        
        # Left face
        glColor3ub(*[int(c*0.7) for c in color])
        glVertex3f(-0.5, -0.5, -0.5)
        glVertex3f(-0.5, -0.5, 0.5)
        glVertex3f(-0.5, 0.5, 0.5)
        glVertex3f(-0.5, 0.5, -0.5)
        
        glEnd()
        glPopMatrix()
        
    def render_selection(self, block_pos):
        """Render outline around selected block"""
        x, y, z = block_pos
        
        glPushMatrix()
        glTranslatef(x + 0.5, y + 0.5, z + 0.5)
        
        glDisable(GL_DEPTH_TEST)
        glColor3f(1, 1, 1)
        glLineWidth(3)
        
        glBegin(GL_LINES)
        # Draw wireframe cube
        for dx in (-0.5, 0.5):
            for dy in (-0.5, 0.5):
                for dz in (-0.5, 0.5):
                    # This is simplified - draw all 12 edges properly
                    pass
        
        # Quick outline - just 12 edges
        edges = [
            (-0.5, -0.5, -0.5), (0.5, -0.5, -0.5),
            (0.5, -0.5, -0.5), (0.5, 0.5, -0.5),
            (0.5, 0.5, -0.5), (-0.5, 0.5, -0.5),
            (-0.5, 0.5, -0.5), (-0.5, -0.5, -0.5),
            
            (-0.5, -0.5, 0.5), (0.5, -0.5, 0.5),
            (0.5, -0.5, 0.5), (0.5, 0.5, 0.5),
            (0.5, 0.5, 0.5), (-0.5, 0.5, 0.5),
            (-0.5, 0.5, 0.5), (-0.5, -0.5, 0.5),
            
            (-0.5, -0.5, -0.5), (-0.5, -0.5, 0.5),
            (0.5, -0.5, -0.5), (0.5, -0.5, 0.5),
            (0.5, 0.5, -0.5), (0.5, 0.5, 0.5),
            (-0.5, 0.5, -0.5), (-0.5, 0.5, 0.5)
        ]
        
        for i in range(0, len(edges), 2):
            glVertex3f(*edges[i])
            glVertex3f(*edges[i+1])
            
        glEnd()
        
        glEnable(GL_DEPTH_TEST)
        glPopMatrix()

# ============================================================================
# Coordinate Display Overlay
# ============================================================================

class CoordinateOverlay:
    """Displays abundant coordinate information on screen"""
    
    def __init__(self, font_size=20):
        pygame.font.init()
        self.font = pygame.font.Font(None, font_size)
        self.small_font = pygame.font.Font(None, font_size - 4)
        
    def render(self, screen, player, world, fps):
        """Render all coordinate information"""
        y_offset = 10
        
        # Player position (floating point)
        pos_text = f"Player Position: X={player.x:.2f} Y={player.y:.2f} Z={player.z:.2f}"
        self.draw_text(screen, pos_text, (10, y_offset), (255, 255, 255))
        y_offset += 25
        
        # Player integer block position
        block_pos = f"Block Position: X={int(player.x)} Y={int(player.y-1)} Z={int(player.z)}"
        self.draw_text(screen, block_pos, (10, y_offset), (200, 200, 255))
        y_offset += 25
        
        # Player rotation
        rot_text = f"Rotation: Yaw={player.rot_y:.1f}° Pitch={player.rot_x:.1f}°"
        self.draw_text(screen, rot_text, (10, y_offset), (255, 255, 200))
        y_offset += 25
        
        # View direction
        dir_vec = player.get_view_direction()
        dir_text = f"View Direction: [{dir_vec[0]:.2f}, {dir_vec[1]:.2f}, {dir_vec[2]:.2f}]"
        self.draw_text(screen, dir_text, (10, y_offset), (200, 255, 200))
        y_offset += 25
        
        # Selected block
        if player.selected_block:
            sx, sy, sz = player.selected_block
            block_type = world.get_block(sx, sy, sz)
            block_name = BLOCK_NAMES.get(block_type, "Unknown")
            face_names = ["+X", "-X", "+Y", "-Y", "+Z", "-Z"]
            face_name = face_names[player.selected_face] if player.selected_face else "?"
            
            select_text = f"Selected: ({sx}, {sy}, {sz}) - {block_name} - Face: {face_name}"
            self.draw_text(screen, select_text, (10, y_offset), (255, 255, 100))
            y_offset += 25
            
            # Block under cursor details
            neighbors = [
                ("Above", sx, sy+1, sz),
                ("Below", sx, sy-1, sz),
                ("North", sx, sy, sz+1),
                ("South", sx, sy, sz-1),
                ("East", sx+1, sy, sz),
                ("West", sx-1, sy, sz)
            ]
            
            for name, nx, ny, nz in neighbors:
                if 0 <= nx < WORLD_SIZE_X and 0 <= ny < WORLD_SIZE_Y and 0 <= nz < WORLD_SIZE_Z:
                    nb_type = world.get_block(nx, ny, nz)
                    nb_name = BLOCK_NAMES.get(nb_type, "Unknown")
                    self.draw_text(screen, f"  {name}: ({nx}, {ny}, {nz}) - {nb_name}", 
                                 (20, y_offset), (200, 200, 200))
                    y_offset += 18
        else:
            self.draw_text(screen, "No block selected", (10, y_offset), (200, 200, 200))
            y_offset += 25
        
        # Chunk information
        cx = int(player.x) // CHUNK_SIZE
        cy = int(player.y-1) // CHUNK_SIZE
        cz = int(player.z) // CHUNK_SIZE
        
        chunk_text = f"Chunk: ({cx}, {cy}, {cz}) | Local: ({int(player.x) % CHUNK_SIZE}, {int(player.y-1) % CHUNK_SIZE}, {int(player.z) % CHUNK_SIZE})"
        self.draw_text(screen, chunk_text, (10, y_offset), (200, 150, 255))
        y_offset += 25
        
        # World statistics
        air_count = 0
        solid_count = 0
        # This would be expensive to compute every frame, so we cache
        # For demo, just show approximate
        world_stats = f"World Size: {WORLD_SIZE_X}x{WORLD_SIZE_Y}x{WORLD_SIZE_Z} | Loaded chunks: {len(world.chunks)}"
        self.draw_text(screen, world_stats, (10, y_offset), (150, 255, 150))
        y_offset += 25
        
        # Performance
        perf_text = f"FPS: {fps:.1f} | Frame time: {1000/fps:.1f}ms"
        self.draw_text(screen, perf_text, (10, y_offset), (255, 150, 150))
        y_offset += 25
        
        # Block in hand
        hand_text = f"Block in hand: {BLOCK_NAMES.get(player.block_in_hand, 'Stone')} (right click to place)"
        self.draw_text(screen, hand_text, (10, y_offset), (255, 200, 150))
        y_offset += 25
        
        # Controls
        controls = [
            "Controls: WASD = move | Mouse = look | Left click = break | Right click = place",
            "Space = jump | F = toggle flight | 1-9 = select block | ESC = quit"
        ]
        for i, ctrl in enumerate(controls):
            self.draw_text(screen, ctrl, (10, WINDOW_HEIGHT - 50 + i*20), (200, 200, 200))
        
        # Compass
        self.draw_compass(screen, player.rot_y)
        
    def draw_text(self, screen, text, pos, color):
        """Draw text on screen"""
        text_surface = self.font.render(text, True, color, (0, 0, 0))
        text_surface.set_colorkey((0, 0, 0))
        screen.blit(text_surface, pos)
        
    def draw_compass(self, screen, yaw):
        """Draw a simple compass"""
        center_x = WINDOW_WIDTH - 100
        center_y = 80
        radius = 40
        
        # Draw compass circle
        pygame.draw.circle(screen, (100, 100, 100), (center_x, center_y), radius, 2)
        pygame.draw.circle(screen, (50, 50, 50), (center_x, center_y), radius - 2)
        
        # Draw directions
        directions = ["N", "E", "S", "W"]
        angles = [0, 90, 180, 270]
        
        for i, (dir_text, angle) in enumerate(zip(directions, angles)):
            rad = math.radians(angle - yaw)
            x = center_x + math.sin(rad) * (radius - 10)
            y = center_y - math.cos(rad) * (radius - 10)
            
            color = (255, 0, 0) if dir_text == "N" else (200, 200, 200)
            text_surface = self.small_font.render(dir_text, True, color)
            text_rect = text_surface.get_rect(center=(x, y))
            screen.blit(text_surface, text_rect)
        
        # Draw player direction indicator
        end_x = center_x + math.sin(math.radians(-yaw)) * radius
        end_y = center_y - math.cos(math.radians(-yaw)) * radius
        pygame.draw.line(screen, (255, 255, 255), (center_x, center_y), (end_x, end_y), 2)

# ============================================================================
# Main Game Class
# ============================================================================

class MinecraftClone:
    """Main game class"""
    
    def __init__(self):
        pygame.init()
        pygame.display.set_mode((WINDOW_WIDTH, WINDOW_HEIGHT), DOUBLEBUF | OPENGL)
        pygame.display.set_caption("Minecraft Clone - Abundant Coordinates")
        pygame.mouse.set_visible(False)
        pygame.event.set_grab(True)
        
        # Initialize components
        self.world = World()
        self.player = Player(self.world)
        self.renderer = Renderer(self.world)
        self.overlay = CoordinateOverlay()
        
        self.clock = pygame.time.Clock()
        self.running = True
        self.fps = 0
        self.frame_count = 0
        self.last_time = time.time()
        
    def handle_events(self):
        """Process pygame events"""
        for event in pygame.event.get():
            if event.type == QUIT:
                self.running = False
                
            elif event.type == KEYDOWN:
                if event.key == K_ESCAPE:
                    self.running = False
                elif event.key == K_f:
                    self.player.flying = not self.player.flying
                elif event.key == K_1:
                    self.player.block_in_hand = BLOCK_STONE
                elif event.key == K_2:
                    self.player.block_in_hand = BLOCK_DIRT
                elif event.key == K_3:
                    self.player.block_in_hand = BLOCK_GRASS
                elif event.key == K_4:
                    self.player.block_in_hand = BLOCK_WOOD
                elif event.key == K_5:
                    self.player.block_in_hand = BLOCK_SAND
                    
            elif event.type == MOUSEMOTION:
                # Mouse look
                dx, dy = event.rel
                self.player.rot_y += dx * MOUSE_SENSITIVITY
                self.player.rot_x += dy * MOUSE_SENSITIVITY
                self.player.rot_x = max(-90, min(90, self.player.rot_x))
                
            elif event.type == MOUSEBUTTONDOWN:
                if event.button == 1:  # Left click - break
                    if self.player.selected_block:
                        x, y, z = self.player.selected_block
                        self.world.set_block(x, y, z, BLOCK_AIR)
                elif event.button == 3:  # Right click - place
                    if self.player.selected_block and self.player.selected_face is not None:
                        x, y, z = self.player.selected_block
                        # Place block adjacent to selected face
                        face = self.player.selected_face
                        if face == 0:  # +X
                            x += 1
                        elif face == 1:  # -X
                            x -= 1
                        elif face == 2:  # +Y
                            y += 1
                        elif face == 3:  # -Y
                            y -= 1
                        elif face == 4:  # +Z
                            z += 1
                        elif face == 5:  # -Z
                            z -= 1
                        
                        if 0 <= x < WORLD_SIZE_X and 0 <= y < WORLD_SIZE_Y and 0 <= z < WORLD_SIZE_Z:
                            self.world.set_block(x, y, z, self.player.block_in_hand)
        
    def update(self):
        """Update game state"""
        dt = self.clock.tick(FPS) / 1000.0
        
        # Get pressed keys
        keys = pygame.key.get_pressed()
        
        # Update player
        self.player.update(dt, keys)
        
        # Update raycast for block selection
        self.player.raycast()
        
        # Calculate FPS
        self.frame_count += 1
        if time.time() - self.last_time >= 1.0:
            self.fps = self.frame_count
            self.frame_count = 0
            self.last_time = time.time()
            
    def render(self):
        """Render the game"""
        # Render 3D world
        self.renderer.render_world(self.player)
        
        # Get the OpenGL surface as a pygame surface for overlay
        pygame.display.flip()
        
        # Create a surface for 2D overlay
        screen_surface = pygame.display.get_surface()
        
        # Draw coordinate overlay
        self.overlay.render(screen_surface, self.player, self.world, self.fps)
        
        # Update display
        pygame.display.flip()
        
    def run(self):
        """Main game loop"""
        while self.running:
            self.handle_events()
            self.update()
            self.render()
            
        pygame.quit()
        sys.exit()

# ============================================================================
# Entry point
# ============================================================================

if __name__ == "__main__":
    game = MinecraftClone()
    game.run()