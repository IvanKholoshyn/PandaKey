package com.pandakey.mobile.di

import android.content.Context
import com.pandakey.mobile.data.api.ApiClient
import com.pandakey.mobile.data.api.ApiService
import com.pandakey.mobile.data.auth.SessionStore
import com.pandakey.mobile.data.repository.AccessRepository
import com.pandakey.mobile.data.repository.AuthRepository
import com.pandakey.mobile.data.repository.EventRepository
import com.pandakey.mobile.data.repository.ProfileRepository
import com.pandakey.mobile.data.repository.ZoneRepository

/**
 * Minimal manual dependency-injection container. Keeps the project free of an
 * external DI framework while still giving ViewModels a single place to obtain
 * their collaborators. Initialized once from [com.pandakey.mobile.PandaKeyApp].
 */
object ServiceLocator {

    private lateinit var appContext: Context

    fun init(context: Context) {
        appContext = context.applicationContext
    }

    val api: ApiService by lazy { ApiClient.create() }

    val sessionStore: SessionStore by lazy { SessionStore(appContext) }

    val authRepository: AuthRepository by lazy { AuthRepository(api, sessionStore) }
    val zoneRepository: ZoneRepository by lazy { ZoneRepository(api) }
    val accessRepository: AccessRepository by lazy { AccessRepository(api) }
    val eventRepository: EventRepository by lazy { EventRepository(api) }
    val profileRepository: ProfileRepository by lazy { ProfileRepository(api) }
}
