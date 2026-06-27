package com.pandakey.mobile.data.auth

import android.content.Context
import androidx.datastore.preferences.core.booleanPreferencesKey
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.intPreferencesKey
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

private val Context.dataStore by preferencesDataStore(name = "pandakey_session")

/**
 * Locally persisted sign-in session. The PandaKey backend has no token-based
 * auth, so a "session" here simply remembers which user the app resolved at
 * login time (by email) and caches a few profile fields for the UI.
 */
data class Session(
    val userId: Int,
    val fullName: String,
    val email: String,
    val departmentId: Int?
)

class SessionStore(private val context: Context) {

    private object Keys {
        val userId = intPreferencesKey("user_id")
        val fullName = stringPreferencesKey("full_name")
        val email = stringPreferencesKey("email")
        val departmentId = intPreferencesKey("department_id")
        val hasDepartment = booleanPreferencesKey("has_department")
        val loggedIn = booleanPreferencesKey("logged_in")
    }

    val session: Flow<Session?> = context.dataStore.data.map { prefs ->
        val loggedIn = prefs[Keys.loggedIn] ?: false
        if (!loggedIn) return@map null
        val id = prefs[Keys.userId] ?: return@map null
        Session(
            userId = id,
            fullName = prefs[Keys.fullName] ?: "",
            email = prefs[Keys.email] ?: "",
            departmentId = if (prefs[Keys.hasDepartment] == true) prefs[Keys.departmentId] else null
        )
    }

    suspend fun save(session: Session) {
        context.dataStore.edit { prefs ->
            prefs[Keys.userId] = session.userId
            prefs[Keys.fullName] = session.fullName
            prefs[Keys.email] = session.email
            if (session.departmentId != null) {
                prefs[Keys.departmentId] = session.departmentId
                prefs[Keys.hasDepartment] = true
            } else {
                prefs[Keys.hasDepartment] = false
            }
            prefs[Keys.loggedIn] = true
        }
    }

    suspend fun clear() {
        context.dataStore.edit { it.clear() }
    }
}
