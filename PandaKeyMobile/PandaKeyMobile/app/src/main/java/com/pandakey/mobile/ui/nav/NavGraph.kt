package com.pandakey.mobile.ui.nav

import androidx.compose.runtime.Composable
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.pandakey.mobile.ui.auth.LoginScreen
import com.pandakey.mobile.ui.auth.RegisterScreen
import com.pandakey.mobile.ui.events.EventsScreen
import com.pandakey.mobile.ui.profile.ProfileScreen
import com.pandakey.mobile.ui.zones.ZoneDetailScreen
import com.pandakey.mobile.ui.zones.ZonesScreen

@Composable
fun PandaKeyNavGraph() {
    val navController = rememberNavController()

    NavHost(navController = navController, startDestination = Routes.LOGIN) {

        composable(Routes.LOGIN) {
            LoginScreen(
                onLoggedIn = {
                    navController.navigate(Routes.ZONES) {
                        popUpTo(Routes.LOGIN) { inclusive = true }
                    }
                },
                onRegister = { navController.navigate(Routes.REGISTER) }
            )
        }

        composable(Routes.REGISTER) {
            RegisterScreen(
                onRegistered = {
                    navController.navigate(Routes.ZONES) {
                        popUpTo(Routes.LOGIN) { inclusive = true }
                    }
                },
                onBack = { navController.popBackStack() }
            )
        }

        composable(Routes.ZONES) {
            ZonesScreen(
                onZoneClick = { zone ->
                    navController.navigate(Routes.zoneDetail(zone.zoneId, zone.name))
                },
                onEvents = { navController.navigate(Routes.EVENTS) },
                onProfile = { navController.navigate(Routes.PROFILE) }
            )
        }

        composable(
            route = Routes.ZONE_DETAIL,
            arguments = listOf(
                navArgument("zoneId") { type = NavType.IntType },
                navArgument("zoneName") { type = NavType.StringType }
            )
        ) { backStackEntry ->
            val zoneId = backStackEntry.arguments?.getInt("zoneId") ?: 0
            val zoneName = backStackEntry.arguments?.getString("zoneName") ?: ""
            ZoneDetailScreen(
                zoneId = zoneId,
                zoneName = zoneName,
                onBack = { navController.popBackStack() }
            )
        }

        composable(Routes.EVENTS) {
            EventsScreen(onBack = { navController.popBackStack() })
        }

        composable(Routes.PROFILE) {
            ProfileScreen(
                onBack = { navController.popBackStack() },
                onLoggedOut = {
                    navController.navigate(Routes.LOGIN) {
                        popUpTo(0) { inclusive = true }
                    }
                }
            )
        }
    }
}
