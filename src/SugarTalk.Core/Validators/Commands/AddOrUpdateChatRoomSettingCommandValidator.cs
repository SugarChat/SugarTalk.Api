using FluentValidation;
using SugarTalk.Core.Middlewares.FluentMessageValidator;
using SugarTalk.Messages.Commands.Meetings.Speak;

namespace SugarTalk.Core.Validators.Commands;

public class AddOrUpdateChatRoomSettingCommandValidator : FluentMessageValidator<AddOrUpdateChatRoomSettingCommand>
{
    public AddOrUpdateChatRoomSettingCommandValidator()
    {
        RuleFor(x => x.SelfLanguage).NotNull();
        RuleFor(x => x.ListeningLanguage).NotNull();
        
        RuleFor(x => x.VoiceId)
            .NotNull().When(x => !x.IsSystem).WithMessage("VoiceId cannot be empty when IsSystem is false.");
    }
   
}