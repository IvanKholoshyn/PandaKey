package com.pandakey.mobile.ui.zones

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.pandakey.mobile.data.api.dto.AccessDecisionResponse
import com.pandakey.mobile.di.ServiceLocator
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch

data class ZoneDetailUiState(
    val loading: Boolean = false,
    val decision: AccessDecisionResponse? = null,
    val error: String? = null,
    val userId: Int? = null,
    val userName: String = ""
)

class ZoneDetailViewModel : ViewModel() {

    private val accessRepository = ServiceLocator.accessRepository
    private val sessionStore = ServiceLocator.sessionStore

    private val _uiState = MutableStateFlow(ZoneDetailUiState())
    val uiState: StateFlow<ZoneDetailUiState> = _uiState.asStateFlow()

    init {
        viewModelScope.launch {
            val session = sessionStore.session.first()
            _uiState.value = _uiState.value.copy(
                userId = session?.userId,
                userName = session?.fullName.orEmpty()
            )
        }
    }

    fun requestAccess(accessPointId: Int) {
        val userId = _uiState.value.userId
        if (userId == null) {
            _uiState.value = _uiState.value.copy(error = "Сесія недоступна")
            return
        }
        _uiState.value = _uiState.value.copy(loading = true, error = null, decision = null)
        viewModelScope.launch {
            runCatching { accessRepository.requestAccess(userId, accessPointId) }
                .onSuccess { _uiState.value = _uiState.value.copy(loading = false, decision = it) }
                .onFailure {
                    _uiState.value = _uiState.value.copy(
                        loading = false,
                        error = it.message ?: "Помилка запиту доступу"
                    )
                }
        }
    }
}
