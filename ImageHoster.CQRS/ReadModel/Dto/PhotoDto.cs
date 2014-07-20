using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel.Dto
{
    public class PhotoDto : IEquatable<PhotoDto>
    {
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public virtual UserDto UploadedBy { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public virtual PrivacySettings Privacy { get; set; }

        public bool Equals(PhotoDto other)
        {
            return this.Id == other.Id;
        }
    }
}
