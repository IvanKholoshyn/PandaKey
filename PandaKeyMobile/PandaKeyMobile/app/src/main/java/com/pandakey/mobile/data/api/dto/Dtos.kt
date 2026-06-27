package com.pandakey.mobile.data.api.dto

import kotlinx.serialization.Serializable

/**
 * Data-transfer objects mirroring the PandaKey ASP.NET Core backend.
 * The backend serializes with camelCase (System.Text.Json default), so the
 * Kotlin property names match the JSON field names directly.
 */

@Serializable
data class UserDto(
    val userId: Int,
    val departmentId: Int? = null,
    val fullName: String,
    val email: String,
    val phone: String? = null,
    val isActive: Boolean = true,
    val createdAt: String? = null
)

@Serializable
data class ZoneDto(
    val zoneId: Int,
    val name: String,
    val description: String? = null
)

@Serializable
data class AccessEventDto(
    val eventId: Long,
    val eventTime: String,
    val userId: Int? = null,
    val accessPointId: Int,
    val credentialId: Int? = null,
    val result: String,
    val reason: String? = null
)

@Serializable
data class CreateUserRequest(
    val departmentId: Int? = null,
    val fullName: String,
    val email: String,
    val phone: String? = null,
    val passwordHash: String
)

@Serializable
data class CreateUserResponse(
    val userId: Int
)

@Serializable
data class AccessDecisionRequest(
    val userId: Int,
    val accessPointId: Int,
    val utcNow: String? = null
)

@Serializable
data class AccessDecisionResponse(
    val granted: Boolean,
    val result: String,
    val reason: String? = null,
    val utc: String? = null
)

@Serializable
data class HealthResponse(
    val ok: Boolean,
    val utc: String? = null,
    val sqlServer: String? = null
)
