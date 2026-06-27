package com.pandakey.mobile.ui.nav

import android.net.Uri

/**
 * Centralized navigation routes. ZONE_DETAIL carries the zone id and a
 * (URL-encoded) display name so the detail screen can show a title without an
 * extra network round-trip.
 */
object Routes {
    const val LOGIN = "login"
    const val REGISTER = "register"
    const val ZONES = "zones"
    const val ZONE_DETAIL = "zone/{zoneId}/{zoneName}"
    const val EVENTS = "events"
    const val PROFILE = "profile"

    fun zoneDetail(zoneId: Int, zoneName: String): String {
        val encoded = Uri.encode(zoneName)
        return "zone/$zoneId/$encoded"
    }
}
