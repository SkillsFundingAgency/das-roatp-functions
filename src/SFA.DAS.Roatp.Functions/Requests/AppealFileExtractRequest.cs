using SFA.DAS.Roatp.Functions.ApplyTypes;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SFA.DAS.Roatp.Functions.Requests
{
    [Serializable]
    public class AppealFileExtractRequest : ISerializable
    {
        public Guid ApplicationId { get; }
        public string FileName { get; }

        public AppealFileExtractRequest(AppealFile appealFile)
        {
            ApplicationId = appealFile.ApplicationId;
            FileName = appealFile.FileName;
        }

        #region Serialization
        // This is the serialization constructor.
        // Satisfies rule CA2229: Implement serialization constructors
        protected AppealFileExtractRequest(SerializationInfo info, StreamingContext context)
        {
            ApplicationId = (Guid)info.GetValue(nameof(ApplicationId), typeof(Guid));
            FileName = info.GetString(nameof(FileName));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ApplicationId), ApplicationId);
            info.AddValue(nameof(FileName), FileName);
        }
        #endregion
    }
}