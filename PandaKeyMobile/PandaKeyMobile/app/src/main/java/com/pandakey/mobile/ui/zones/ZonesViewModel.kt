package com.pandakey.mobile.ui.zones

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.pandakey.mobile.data.api.dto.ZoneDto
import com.pandakey.mobile.di.ServiceLocator
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch

data class ZonesUiState(
    val loading: Boolean = false,
    val zones: List<ZoneDto> = emptyList(),
    val error: String? = null,
    val greetingName: String = ""
)

class ZonesViewModel : ViewModel() {

    private val zoneRepository = ServiceLocator.zoneRepository
    private val sessionStore = ServiceLocator.sessionStore

    private val _uiState = MutableStateFlow(ZonesUiState(loading = true))
    val uiState: StateFlow<ZonesUiState> = _uiState.asStateFlow()

    init {
        load()
    }

    fun load() {
        _uiState.value = _uiState.value.copy(loading = true, error = null)
        viewModelScope.launch {
            val session = sessionStore.session.first()
            // Greeting uses the given name. Ukrainian full names are usually
            // "Прізвище Ім'я По-батькові", so the second token is the first name.
            val greeting = session?.fullName
                ?.trim()
                ?.split(" ")
                ?.getOrNull(1)
                ?: session?.fullName?.trim().orEmpty()

            runCatching { zoneRepository.zones(top = 50) }
                .onSuccess { list ->
                    _uiState.value = ZonesUiState(
                        loading = false,
                        zones = list,
                        greetingName = greeting
                    )
                }
                .onFailure {
                    _uiState.value = ZonesUiState(
                        loading = false,
                        error = it.message ?: "Не вдалося завантажити зони",
                        greetingName = greeting
                    )
                }
        }
    }
}
