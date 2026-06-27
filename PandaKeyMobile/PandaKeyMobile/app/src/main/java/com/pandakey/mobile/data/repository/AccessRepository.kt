package com.pandakey.mobile.data.repository

import com.pandakey.mobile.data.api.ApiService
import com.pandakey.mobile.data.api.dto.AccessDecisionRequest
import com.pandakey.mobile.data.api.dto.AccessDecisionResponse

/**
 * Requests an access decision. The backend evaluates active access rules and
 * schedule intervals, returns granted/denied with a reason, and also records
 * the attempt as an AccessEvent.
 */
class AccessRepository(private val api: ApiService) {
    suspend fun requestAccess(userId: Int, accessPointId: Int): AccessDecisionResponse =
        api.decide(AccessDecisionRequest(userId = userId, accessPointId = accessPointId))
}
