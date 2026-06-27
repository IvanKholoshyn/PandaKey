package com.pandakey.mobile.data.repository

import com.pandakey.mobile.data.api.ApiService
import com.pandakey.mobile.data.api.dto.AccessEventDto

class EventRepository(private val api: ApiService) {
    suspend fun latest(top: Int = 50): List<AccessEventDto> = api.getAccessEvents(top)
}
