namespace App.Authorization.References;

/*! A centralized repo for role names. */
public static class RoleReference
{
	public const string Admin = "Admin";
	public const string Staff = "Staff";
}

/*! A centralized repo for policy names. */
public static class PolicyReference
{
	public const string AdminOnly = "AdminOnlyPolicy";
	public const string AccreditedOnly = "AccreditedOnlyPolicy";
}
