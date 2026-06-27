package com.pandakey.mobile

import android.app.Application
import com.pandakey.mobile.di.ServiceLocator

/**
 * Application entry point. Initializes the lightweight manual DI container
 * (ServiceLocator) so repositories and the API client are available app-wide.
 */
class PandaKeyApp : Application() {
    override fun onCreate() {
        super.onCreate()
        ServiceLocator.init(this)
    }
}
