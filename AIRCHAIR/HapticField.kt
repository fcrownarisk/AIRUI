// AIRCHAIR.kt - Haptic Field System
package com.airchair.haptic

import com.airtouch.core.VoidInteraction
import kotlinx.coroutines.*
import kotlin.math.*

/**
 * Haptic Field - Creates tactile feedback through quantum resonance
 * "Feel the void respond to your presence"
 */
class HapticField(
    private val voidSystem: VoidInteraction,
    private val config: HapticConfig
) {
    companion object {
        private const val HAPTIC_RESOLUTION = 256
        private const val RESONANCE_FREQUENCY = 7.83 // Hz
        private const val MAX_INTENSITY = 1000 // milli-g
    }
    
    // Haptic transducer array
    private val transducers = Array(HAPTIC_RESOLUTION) { i ->
        HapticTransducer(
            id = i,
            position = calculatePosition(i),
            frequencyRange = 20.0..1000.0,
            maxAmplitude = MAX_INTENSITY.toDouble()
        )
    }
    
    // Field state
    private val hapticField = MutableStateFlow(Array(HAPTIC_RESOLUTION) { 0.0 })
    private val resonancePattern = MutableStateFlow(ResonancePattern())
    
    /**
     * Generate haptic field from touch data
     */
    suspend fun generateField(touchPoints: List<VoidInteraction.TouchPoint>): HapticPattern {
        val field = Array(HAPTIC_RESOLUTION) { 0.0 }
        
        // Superimpose touch influences
        touchPoints.forEach { touch ->
            transducers.forEachIndexed { index, transducer ->
                val distance = calculateDistance(touch, transducer.position)
                val influence = touch.pressure * exp(-distance / 100.0)
                field[index] = (field[index] + influence).coerceIn(0.0, 1.0)
            }
        }
        
        // Apply quantum resonance
        val resonance = calculateResonance(field)
        val modulatedField = field.map { it * resonance }
        
        hapticField.value = modulatedField.toTypedArray()
        
        return HapticPattern(
            intensities = modulatedField.toDoubleArray(),
            resonance = resonance,
            frequency = RESONANCE_FREQUENCY,
            duration = 100 // ms
        )
    }
    
    /**
     * Apply haptic pattern to transducers
     */
    suspend fun applyPattern(pattern: HapticPattern) {
        transducers.parallelStream().forEachIndexed { index, transducer ->
            val intensity = pattern.intensities[index]
            if (intensity > 0.01) {
                transducer.actuate(
                    frequency = pattern.frequency,
                    amplitude = intensity * transducer.maxAmplitude,
                    duration = pattern.duration
                )
            }
        }
    }
    
    /**
     * Create directional haptic sensation
     */
    suspend fun createDirectionalPull(
        direction: Vector3,
        intensity: Double
    ): HapticPattern {
        val field = Array(HAPTIC_RESOLUTION) { index ->
            val pos = transducers[index].position
            val dot = direction.x * pos.x + direction.y * pos.y
            max(0.0, dot) * intensity
        }
        
        return HapticPattern(
            intensities = field.toDoubleArray(),
            resonance = 1.0,
            frequency = 100.0,
            duration = 500
        )
    }
    
    private fun calculatePosition(index: Int): Vector3 {
        // 16x16 grid arrangement
        val x = (index % 16) * 50.0
        val y = (index / 16) * 50.0
        return Vector3(x, y, 0.0)
    }
    
    private fun calculateDistance(touch: VoidInteraction.TouchPoint, pos: Vector3): Double {
        val dx = touch.x - pos.x
        val dy = touch.y - pos.y
        return sqrt(dx*dx + dy*dy)
    }
    
    private fun calculateResonance(field: Array<Double>): Double {
        val totalIntensity = field.sum()
        return sin(2 * PI * RESONANCE_FREQUENCY * totalIntensity)
    }
    
    data class HapticTransducer(
        val id: Int,
        val position: Vector3,
        val frequencyRange: ClosedRange<Double>,
        val maxAmplitude: Double
    ) {
        suspend fun actuate(frequency: Double, amplitude: Double, duration: Long) {
            // Transducer actuation logic
            delay(duration)
        }
    }
    
    data class HapticPattern(
        val intensities: DoubleArray,
        val resonance: Double,
        val frequency: Double,
        val duration: Long
    )
    
    data class ResonancePattern(
        val frequency: Double = RESONANCE_FREQUENCY,
        val phase: Double = 0.0,
        val amplitude: Double = 1.0
    )
}