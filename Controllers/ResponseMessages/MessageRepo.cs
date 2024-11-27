namespace App.Controllers.ResponseMessages;

public static class MessageRepo
{
	public const string BadTemplate = "Given JSON template is unacceptable.";
	public const string UnfitData = "Given JSON data does not fit predefined template.";
	public const string DeadlineReached = "Deadline for this action-on-resource has passed.";
	public const string InactiveResource = "Target resource is inactive.";
	public const string MaxObjCountReached = "Too many objects of this kind.";
	public const string PasscodeError = "Provide the correct passcode to run this action-on-resource.";
	public const string UpdateConflict = "The resource has been modified since your last read.";
}
