//! Puck's exact netcode contract, extracted from `Puck.dll` via Mono.Cecil
//! (methods/fields in metadata-token order = ILPP table order).
//!
//! 56 RPCs, 34 NetworkVariables across 12 behaviours. `None` entries are
//! client-bound; the server dispatches every id it receives and verifies
//! direction against live traffic.

/// (behaviour, method id, name) for every RPC Puck declares, metadata order.
pub const RPCS: &[(&str, u32, &str)] = &[
    ("PlayerInput", 0, "Client_MoveInputRpc"),
    ("PlayerInput", 1, "Server_MoveInputRpc"),
    ("PlayerInput", 2, "Client_RaycastOriginAngleInputRpc"),
    ("PlayerInput", 3, "Server_RaycastOriginAngleInputRpc"),
    ("PlayerInput", 4, "Client_LookAngleInputRpc"),
    ("PlayerInput", 5, "Server_LookAngleInputRpc"),
    ("PlayerInput", 6, "Client_BladeAngleInputRpc"),
    ("PlayerInput", 7, "Server_BladeAngleInputRpc"),
    ("PlayerInput", 8, "Client_SlideInputRpc"),
    ("PlayerInput", 9, "Server_SlideInputRpc"),
    ("PlayerInput", 10, "Client_SprintInputRpc"),
    ("PlayerInput", 11, "Server_SprintInputRpc"),
    ("PlayerInput", 12, "Client_TrackInputRpc"),
    ("PlayerInput", 13, "Server_TrackInputRpc"),
    ("PlayerInput", 14, "Client_LookInputRpc"),
    ("PlayerInput", 15, "Server_LookInputRpc"),
    ("PlayerInput", 16, "Client_JumpInputRpc"),
    ("PlayerInput", 17, "Client_StopInputRpc"),
    ("PlayerInput", 18, "Client_DashLeftInputRpc"),
    ("PlayerInput", 19, "Client_DashRightInputRpc"),
    ("PlayerInput", 20, "Client_TwistLeftInputRpc"),
    ("PlayerInput", 21, "Client_TwistRightInputRpc"),
    ("PlayerInput", 22, "Client_ExtendLeftInputRpc"),
    ("PlayerInput", 23, "Server_ExtendLeftInputRpc"),
    ("PlayerInput", 24, "Client_ExtendRightInputRpc"),
    ("PlayerInput", 25, "Server_ExtendRightInputRpc"),
    ("PlayerInput", 26, "Client_LateralLeftInputRpc"),
    ("PlayerInput", 27, "Server_LateralLeftInputRpc"),
    ("PlayerInput", 28, "Client_LateralRightInputRpc"),
    ("PlayerInput", 29, "Server_LateralRightInputRpc"),
    ("PlayerInput", 30, "Client_TalkInputRpc"),
    ("PlayerInput", 31, "Server_TalkInputRpc"),
    ("PlayerVoiceRecorder", 0, "Client_RequestVoiceStartRpc"),
    ("PlayerVoiceRecorder", 1, "Server_VoiceStartRpc"),
    ("PlayerVoiceRecorder", 2, "Client_VoiceDataRpc"),
    ("PlayerVoiceRecorder", 3, "Server_VoiceDataRpc"),
    ("PlayerVoiceRecorder", 4, "Client_RequestVoiceStopRpc"),
    ("PlayerVoiceRecorder", 5, "Server_VoiceStopRpc"),
    ("Player", 0, "Client_RequestTeamRpc"),
    ("Player", 1, "Client_RequestClaimPositionRpc"),
    ("Player", 2, "Client_RequestTeamSelectRpc"),
    ("Player", 3, "Client_RequestPositionSelectRpc"),
    ("Player", 4, "Client_RequestForfeitRpc"),
    ("Player", 5, "Client_RequestHandednessRpc"),
    ("SynchronizedAudio", 0, "Server_PlayRpc"),
    ("ChatManager", 0, "Client_SendChatMessageRpc"),
    ("ChatManager", 1, "Server_SendChatMessageRpc"),
    ("GameManager", 0, "Server_NotifySlowMotionRpc"),
    ("GameManager", 1, "Server_NotifyReplayCellyCamRpc"),
    ("GameManager", 2, "Server_NotifySlowMotionStopRpc"),
    ("GameManager", 3, "Server_NotifyGoalScoredRpc"),
    ("SynchronizedObjectManager", 0, "Server_SynchronizeObjectsRpc"),
    ("SynchronizedObjectManager", 1, "Server_ReliableSynchronizeObjectsRpc"),
    ("VoteManager", 0, "Server_NotifyVoteStartedRpc"),
    ("VoteManager", 1, "Server_NotifyVoteProgressedRpc"),
    ("VoteManager", 2, "Server_NotifyVoteEndedRpc"),
];

/// (behaviour, field index, name) for every NetworkVariable, metadata order.
pub const NETVARS: &[(&str, u32, &str)] = &[
    ("PlayerPosition", 0, "ClaimedByPlayerReference"),
    ("PlayerBody", 0, "PlayerReference"),
    ("PlayerBody", 1, "Stamina"),
    ("PlayerBody", 2, "Speed"),
    ("PlayerBody", 3, "IsSprinting"),
    ("PlayerBody", 4, "IsSliding"),
    ("PlayerBody", 5, "IsStopping"),
    ("PlayerBody", 6, "IsExtendedLeft"),
    ("PlayerBody", 7, "IsExtendedRight"),
    ("PlayerBody", 8, "HasFallen"),
    ("PlayerCamera", 0, "PlayerReference"),
    ("Player", 0, "GameState"),
    ("Player", 1, "CustomizationState"),
    ("Player", 2, "Handedness"),
    ("Player", 3, "SteamId"),
    ("Player", 4, "Username"),
    ("Player", 5, "Number"),
    ("Player", 6, "PatreonLevel"),
    ("Player", 7, "AdminLevel"),
    ("Player", 8, "Goals"),
    ("Player", 9, "Assists"),
    ("Player", 10, "Ping"),
    ("Player", 11, "PlayerPositionReference"),
    ("Player", 12, "IsMuted"),
    ("Player", 13, "IsReplay"),
    ("Stick", 0, "PlayerReference"),
    ("StickPositioner", 0, "PlayerReference"),
    ("Puck", 0, "IsReplay"),
    ("SpectatorCamera", 0, "PlayerReference"),
    ("SynchronizedAudio", 0, "Volume"),
    ("SynchronizedAudio", 1, "Pitch"),
    ("GameManager", 0, "GameState"),
    ("GameModeManager", 0, "ClientConfig"),
    ("ServerManager", 0, "Server"),
];

/// Look up an RPC name by behaviour + method id.
pub fn rpc_name(behaviour: &str, id: u32) -> Option<&'static str> {
    RPCS.iter()
        .find(|(b, i, _)| *b == behaviour && *i == id)
        .map(|(_, _, n)| *n)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn contract_counts_match_dll_inventory() {
        assert_eq!(RPCS.len(), 56);
        assert_eq!(NETVARS.len(), 34);
    }

    #[test]
    fn key_ids_spot_check() {
        assert_eq!(rpc_name("PlayerInput", 1), Some("Server_MoveInputRpc"));
        assert_eq!(rpc_name("ChatManager", 1), Some("Server_SendChatMessageRpc"));
        assert_eq!(rpc_name("Player", 99), None);
    }
}
