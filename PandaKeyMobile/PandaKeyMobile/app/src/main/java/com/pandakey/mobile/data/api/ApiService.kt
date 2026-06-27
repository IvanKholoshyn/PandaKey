package com.pandakey.mobile.data.api

import com.pandakey.mobile.data.api.dto.AccessDecisionRequest
import com.pandakey.mobile.data.api.dto.AccessDecisionResponse
import com.pandakey.mobile.data.api.dto.AccessEventDto
import com.pandakey.mobile.data.api.dto.CreateUserRequest
import com.pandakey.mobile.data.api.dto.CreateUserResponse
import com.pandakey.mobile.data.api.dto.HealthResponse
import com.pandakey.mobile.data.api.dto.UserDto
import com.pandakey.mobile.data.api.dto.ZoneDto
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path
import retrofit2.http.Query

/**
 * Retrofit description of the PandaKey HTTP API. Endpoint paths and shapes
 * follow the backend controllers exactly; no authentication header is required
 * because the backend currently exposes the API without JWT/auth.
 */
interface ApiService {

    @GET("api/users")
    suspend fun getUsers(@Query("top") top: Int = 50): List<UserDto>

    @GET("api/users/{id}")
    suspend fun getUser(@Path("id") id: Int): UserDto

    @POST("api/users")
    suspend fun createUser(@Body request: CreateUserRequest): CreateUserResponse

    @GET("api/zones")
    suspend fun getZones(@Query("top") top: Int = 50): List<ZoneDto>

    @GET("api/access-events")
    suspend fun getAccessEvents(@Query("top") top: Int = 50): List<AccessEventDto>

    @POST("api/access/decide")
    suspend fun decide(@Body request: AccessDecisionRequest): AccessDecisionResponse

    @GET("api/health/db")
    suspend fun healthDb(): HealthResponse
}
