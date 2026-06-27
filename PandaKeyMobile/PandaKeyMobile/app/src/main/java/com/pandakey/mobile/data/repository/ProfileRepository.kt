package com.pandakey.mobile.data.repository

import com.pandakey.mobile.data.api.ApiService
import com.pandakey.mobile.data.api.dto.HealthResponse
import com.pandakey.mobile.data.api.dto.UserDto

class ProfileRepository(private val api: ApiService) {
    suspend fun user(id: Int): UserDto = api.getUser(id)
    suspend fun health(): HealthResponse = api.healthDb()
}
