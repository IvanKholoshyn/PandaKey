package com.pandakey.mobile.ui.events

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.pandakey.mobile.data.api.dto.AccessEventDto
import com.pandakey.mobile.di.ServiceLocator
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch

data class EventsUiState(
    val loading: Boolean = false,
    val all: List<AccessEventDto> = emptyList(),
    val mineOnly: Boolean = false,
    val userId: Int? = null,
    val error: String? = null
)

class EventsViewModel : ViewModel() {

    private val eventRepository = ServiceLocator.eventRepository
    private val sessionStore = ServiceLocator.sessionStore

    private val _uiState = MutableStateFlow(EventsUiState(loading = true))
    val uiState: StateFlow<EventsUiState> = _uiState.asStateFlow()

    init {
        load()
    }

    fun load() {
        _uiState.value = _uiState.value.copy(loading = true, error = null)
        viewModelScope.launch {
            val session = sessionStore.session.first()
            runCatching { eventRepository.latest(top = 100) }
                .onSuccess { list ->
                    _uiState.value = _uiState.value.copy(
                        loading = false,
                        all = list,
                        userId = session?.userId
                    )
                }
                .onFailure {
                    _uiState.value = _uiState.value.copy(
                        loading = false,
                        error = it.message ?: "Не вдалося завантажити журнал",
                        userId = session?.userId
                    )
                }
        }
    }

    fun toggleMine(mineOnly: Boolean) {
        _uiState.value = _uiState.value.copy(mineOnly = mineOnly)
    }
}
