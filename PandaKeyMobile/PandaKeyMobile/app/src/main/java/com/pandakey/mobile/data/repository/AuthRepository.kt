package com.pandakey.mobile.data.repository

import com.pandakey.mobile.data.api.ApiService
import com.pandakey.mobile.data.api.dto.CreateUserRequest
import com.pandakey.mobile.data.auth.Session
import com.pandakey.mobile.data.auth.SessionStore
import kotlinx.coroutines.flow.Flow

/**
 * Authentication-style operations on top of the PandaKey backend.
 *
 * The backend exposes no login endpoint or JWT issuance, so this app implements
 * a pragmatic equivalent:
 *  - register: POST /api/users, then load the created user and open a session;
 *  - login: look the user up by email through GET /api/users and, if the account
 *    is active, open a local session.
 *
 * The password is only sent at registration (stored server-side as passwordHash);
 * because there is no verify endpoint, login is identity resolution by email.
 */
class AuthRepository(
    private val api: ApiService,
    private val sessionStore: SessionStore
) {

    val session: Flow<Session?> = sessionStore.session

    suspend fun login(email: String) {
        val normalized = email.trim()
        val users = api.getUsers(top = 200)
        val match = users.firstOrNull { it.email.equals(normalized, ignoreCase = true) }
            ?: throw IllegalStateException("Користувача з такою поштою не знайдено")
        if (!match.isActive) {
            throw IllegalStateException("Обліковий запис деактивовано")
        }
        sessionStore.save(
            Session(
                userId = match.userId,
                fullName = match.fullName,
                email = match.email,
                departmentId = match.departmentId
            )
        )
    }

    suspend fun register(fullName: String, email: String, phone: String?, password: String) {
        val created = api.createUser(
            CreateUserRequest(
                fullName = fullName.trim(),
                email = email.trim(),
                phone = phone?.trim()?.ifBlank { null },
                passwordHash = password
            )
        )
        val user = api.getUser(created.userId)
        sessionStore.save(
            Session(
                userId = user.userId,
                fullName = user.fullName,
                email = user.email,
                departmentId = user.departmentId
            )
        )
    }

    suspend fun logout() {
        sessionStore.clear()
    }
}
