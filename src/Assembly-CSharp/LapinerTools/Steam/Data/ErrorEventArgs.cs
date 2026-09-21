using Steamworks;

namespace LapinerTools.Steam.Data;

public class ErrorEventArgs : EventArgsBase
{
	public const string ERROR_MSG_STEAM_NOT_INIT = "Steam must be running!";

	public const string ERROR_MSG_WORKSHOP_LEGAL_AGREEMENT = "Please accept Steam Workshop legal agreement first!";

	public static string ERROR_MSG(EResult p_result)
	{
		return p_result switch
		{
			EResult.k_EResultFail => "Generic failure!", 
			EResult.k_EResultNoConnection => "No/failed network connection!", 
			EResult.k_EResultInvalidPassword => "Password/ticket is invalid!", 
			EResult.k_EResultLoggedInElsewhere => "Same user logged in elsewhere!", 
			EResult.k_EResultInvalidProtocolVer => "Protocol version is incorrect!", 
			EResult.k_EResultInvalidParam => "A parameter is incorrect!", 
			EResult.k_EResultFileNotFound => "File was not found!", 
			EResult.k_EResultBusy => "Called method busy - action not taken!", 
			EResult.k_EResultInvalidState => "Called object was in an invalid state!", 
			EResult.k_EResultInvalidName => "Name is invalid!", 
			EResult.k_EResultInvalidEmail => "E-mail is invalid!", 
			EResult.k_EResultDuplicateName => "Name is not unique!", 
			EResult.k_EResultAccessDenied => "Access is denied!", 
			EResult.k_EResultTimeout => "Operation timed out!", 
			EResult.k_EResultBanned => "VAC2 banned!", 
			EResult.k_EResultAccountNotFound => "Account not found!", 
			EResult.k_EResultInvalidSteamID => "SteamID is invalid!", 
			EResult.k_EResultServiceUnavailable => "The requested service is currently unavailable!", 
			EResult.k_EResultNotLoggedOn => "The user is not logged on!", 
			EResult.k_EResultPending => "Request is pending (may be in process, or waiting on third party)!", 
			EResult.k_EResultEncryptionFailure => "Encryption or decryption failed!", 
			EResult.k_EResultInsufficientPrivilege => "Insufficient privilege!", 
			EResult.k_EResultLimitExceeded => "Limit exceeded!", 
			EResult.k_EResultRevoked => "Access has been revoked!", 
			EResult.k_EResultExpired => "License/guest pass is expired!", 
			EResult.k_EResultAlreadyRedeemed => "Guest pass has already been redeemed by account, cannot be used again!", 
			EResult.k_EResultDuplicateRequest => "The request is a duplicate and the action has already occurred in the past, ignored this time!", 
			EResult.k_EResultAlreadyOwned => "All the games in this guest pass redemption request are already owned!", 
			EResult.k_EResultIPNotFound => "IP address not found!", 
			EResult.k_EResultPersistFailed => "Failed to write change to the data store!", 
			EResult.k_EResultLockingFailed => "Failed to acquire access lock for this operation!", 
			EResult.k_EResultLogonSessionReplaced => "Logon session replaced!", 
			EResult.k_EResultConnectFailed => "Connect failed!", 
			EResult.k_EResultHandshakeFailed => "Handshake failed!", 
			EResult.k_EResultIOFailure => "IO failure!", 
			EResult.k_EResultRemoteDisconnect => "Remote disconnect!", 
			EResult.k_EResultShoppingCartNotFound => "Failed to find the shopping cart requested!", 
			EResult.k_EResultBlocked => "A user didn't allow it!", 
			EResult.k_EResultIgnored => "Target is ignoring sender!", 
			EResult.k_EResultNoMatch => "Nothing matching the request found!", 
			EResult.k_EResultAccountDisabled => "Account disabled!", 
			EResult.k_EResultServiceReadOnly => "This service is not accepting content changes right now!", 
			EResult.k_EResultAccountNotFeatured => "Account doesn't have value, so this feature isn't available!", 
			EResult.k_EResultAdministratorOK => "Allowed to take this action, but only because requester is admin!", 
			EResult.k_EResultContentVersion => "A Version mismatch in content transmitted within the Steam protocol!", 
			EResult.k_EResultTryAnotherCM => "The current CM can't service the user making a request, user should try another!", 
			EResult.k_EResultPasswordRequiredToKickSession => "You are already logged in elsewhere, this cached credential login has failed!", 
			EResult.k_EResultAlreadyLoggedInElsewhere => "You are already logged in elsewhere, you must wait!", 
			EResult.k_EResultSuspended => "Long running operation (content download) suspended/paused!", 
			EResult.k_EResultCancelled => "Operation canceled!", 
			EResult.k_EResultDataCorruption => "Operation canceled, because data is ill formed or unrecoverable!", 
			EResult.k_EResultDiskFull => "Operation canceled, because not enough disk space!", 
			EResult.k_EResultRemoteCallFailed => "A remote call or IPC call failed!", 
			EResult.k_EResultPasswordUnset => "Password could not be verified as it's unset server side!", 
			EResult.k_EResultExternalAccountUnlinked => "External account (PSN, Facebook...) is not linked to a Steam account!", 
			EResult.k_EResultPSNTicketInvalid => "PSN ticket was invalid!", 
			EResult.k_EResultExternalAccountAlreadyLinked => "External account (PSN, Facebook...) is already linked to some other account, must explicitly request to replace/delete the link first!", 
			EResult.k_EResultRemoteFileConflict => "The sync cannot resume due to a conflict between the local and remote files!", 
			EResult.k_EResultIllegalPassword => "The requested new password is not legal!", 
			EResult.k_EResultSameAsPreviousValue => "New value is the same as the old one!", 
			EResult.k_EResultAccountLogonDenied => "Account login denied due to 2nd factor authentication failure!", 
			EResult.k_EResultCannotUseOldPassword => "The requested new password is not legal!", 
			EResult.k_EResultInvalidLoginAuthCode => "Account login denied due to auth code invalid!", 
			EResult.k_EResultAccountLogonDeniedNoMail => "Account login denied due to 2nd factor auth failure - and no mail has been sent!", 
			EResult.k_EResultHardwareNotCapableOfIPT => "Hardware not capable of IPT!", 
			EResult.k_EResultIPTInitError => "IPT init error!", 
			EResult.k_EResultParentalControlRestricted => "Operation failed due to parental control restrictions for current user!", 
			EResult.k_EResultFacebookQueryError => "Facebook query returned an error!", 
			EResult.k_EResultExpiredLoginAuthCode => "Account login denied due to auth code expired!", 
			EResult.k_EResultIPLoginRestrictionFailed => "IP login restriction failed!", 
			EResult.k_EResultAccountLockedDown => "Account locked down!", 
			EResult.k_EResultAccountLogonDeniedVerifiedEmailRequired => "Account logon denied, verified e-mail required!", 
			EResult.k_EResultNoMatchingURL => "No matching URL!", 
			EResult.k_EResultBadResponse => "Parse failure, missing field, etc!", 
			EResult.k_EResultRequirePasswordReEntry => "The user cannot complete the action until they re-enter their password!", 
			EResult.k_EResultValueOutOfRange => "The value entered is outside the acceptable range!", 
			EResult.k_EResultUnexpectedError => "Something happened that we didn't expect to ever happen!", 
			EResult.k_EResultDisabled => "The requested service has been configured to be unavailable!", 
			EResult.k_EResultInvalidCEGSubmission => "The set of files submitted to the CEG server are not valid!", 
			EResult.k_EResultRestrictedDevice => "The device being used is not allowed to perform this action!", 
			EResult.k_EResultRegionLocked => "The action could not be complete because it is region restricted!", 
			EResult.k_EResultRateLimitExceeded => "Temporary rate limit exceeded, try again later!", 
			EResult.k_EResultAccountLoginDeniedNeedTwoFactor => "Need two-factor code to login!", 
			EResult.k_EResultItemDeleted => "The thing we're trying to access has been deleted!", 
			EResult.k_EResultAccountLoginDeniedThrottle => "Login attempt failed, try to throttle response to possible attacker!", 
			EResult.k_EResultTwoFactorCodeMismatch => "Two factor code mismatch!", 
			EResult.k_EResultTwoFactorActivationCodeMismatch => "Activation code for two-factor didn't match!", 
			EResult.k_EResultAccountAssociatedToMultiplePartners => "Account has been associated with multiple partners!", 
			EResult.k_EResultNotModified => "Data not modified!", 
			EResult.k_EResultNoMobileDevice => "The account does not have a mobile device associated with it!", 
			EResult.k_EResultTimeNotSynced => "The time presented is out of range or tolerance!", 
			EResult.k_EResultSmsCodeFailed => "SMS code failure (no match, none pending, etc.)!", 
			EResult.k_EResultAccountLimitExceeded => "Too many accounts access this resource!", 
			EResult.k_EResultAccountActivityLimitExceeded => "Too many changes to this account!", 
			EResult.k_EResultPhoneActivityLimitExceeded => "Too many changes to this phone!", 
			EResult.k_EResultRefundToWallet => "Cannot refund to payment method, must use wallet!", 
			EResult.k_EResultEmailSendFailure => "Cannot send an e-mail!", 
			EResult.k_EResultNotSettled => "Can't perform operation till payment has settled!", 
			_ => "Internal error!", 
		};
	}

	public static ErrorEventArgs CreateSteamNotInit()
	{
		return new ErrorEventArgs("Steam must be running!");
	}

	public static ErrorEventArgs CreateWorkshopLegalAgreement()
	{
		return new ErrorEventArgs("Please accept Steam Workshop legal agreement first!");
	}

	public static ErrorEventArgs Create(EResult p_result)
	{
		return new ErrorEventArgs(ERROR_MSG(p_result));
	}

	public ErrorEventArgs()
	{
		base.IsError = true;
	}

	public ErrorEventArgs(string p_errorMessage)
	{
		base.IsError = true;
		base.ErrorMessage = p_errorMessage;
	}
}
