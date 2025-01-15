namespace App.Controllers.ResponseMessages;

public static class MessageRepo
{
	public const string BadTemplate = "Given JSON template is unacceptable.";
	public const string InactiveResource = "Target resource is inactive.";
	public const string MaxGuessCountReached = "Maximum guess count has been already reached.";
	public const string PasscodeError = "Provided passcode is wrong.";
	public const string SubsDeadlineReached = "Submission deadline has passed.";
	public const string TooEarlySubsDeadline = "Submission deadline must be at least 5 minutes in future.";
	public const string UnfitData = "Given JSON data does not fit predefined template.";
	public const string UpdateConflict = "The resource has been modified since your last read.";
}
