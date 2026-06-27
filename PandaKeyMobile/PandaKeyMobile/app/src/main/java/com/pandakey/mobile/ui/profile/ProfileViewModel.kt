package com.pandakey.mobile.ui.profile

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.pandakey.mobile.data.api.dto.HealthResponse
import com.pandakey.mobile.data.api.dto.UserDto
import com.pandakey.mobile.di.ServiceLocator
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch

data class ProfileUiState(
    val loading: Boolean = false,
    val user: UserDto? = null,
    val health: HealthResponse? = null,
    val error: String? = null
)

class ProfileViewModel : ViewModel() {

    private val profileRepository = ServiceLocator.profileRepository
    private val authRepository = ServiceLocator.authRepository
    private val sessionStore = ServiceLocator.sessionStore

    private val _uiState = MutableStateFlow(ProfileUiState(loading = true))
    val uiState: StateFlow<ProfileUiState> = _uiState.asStateFlow()

    init {
        load()
    }

    fun load() {
        _uiState.value = _uiState.value.copy(loading = true, error = null)
        viewModelScope.launch {
            val session = sessionStore.session.first()
            val userId = session?.userId
            if (userId == null) {
                _uiState.value = ProfileUiState(loading = false, error = "Сесія недоступна")
                return@launch
            }
            val user = runCatching { profileRepository.user(userId) }.getOrNull()
            val health = runCatching { profileRepository.health() }.getOrNull()
            _uiState.value = ProfileUiState(
                loading = false,
                user = user,
                health = health,
                error = if (user == null) "Не вдалося завантажити профіль" else null
            )
        }
    }

    fun logout(onDone: () -> Unit) {
        viewModelScope.launch {
            authRepository.logout()
            onDone()
        }
    }
}
