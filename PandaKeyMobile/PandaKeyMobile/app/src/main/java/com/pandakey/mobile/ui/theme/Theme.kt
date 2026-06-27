package com.pandakey.mobile.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable

private val PandaColorScheme = lightColorScheme(
    primary = PandaPrimary,
    onPrimary = PandaOnPrimary,
    primaryContainer = PandaPrimaryDark,
    onPrimaryContainer = PandaOnPrimary,
    background = PandaBackground,
    surface = PandaSurface,
    error = PandaDanger
)

@Composable
fun PandaKeyTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = PandaColorScheme,
        typography = PandaTypography,
        content = content
    )
}
