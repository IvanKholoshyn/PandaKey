package com.pandakey.mobile.data.repository

import com.pandakey.mobile.data.api.ApiService
import com.pandakey.mobile.data.api.dto.ZoneDto

class ZoneRepository(private val api: ApiService) {
    suspend fun zones(top: Int = 50): List<ZoneDto> = api.getZones(top)
}
