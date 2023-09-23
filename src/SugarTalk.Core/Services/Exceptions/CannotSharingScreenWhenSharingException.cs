using System;

namespace SugarTalk.Core.Services.Exceptions;

public class CannotSharingScreenWhenSharingException : Exception
{
    public CannotSharingScreenWhenSharingException() : base("The current user cannot sharing screen when other user sharing.")
    {
    }
}