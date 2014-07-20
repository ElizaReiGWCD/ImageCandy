using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS
{
    public enum Sex
    {
        Man,
        Woman,
        Other
    }

    public enum PrivacyLevel
    {
        Public,
        Hidden,
        Group,
        Private
    }

    public enum GroupPrivacyLevel
    {
        Public,
        SemiPublic,
        Private
    }
}
