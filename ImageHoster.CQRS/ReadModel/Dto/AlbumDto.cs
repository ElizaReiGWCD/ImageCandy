using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.ReadModel.Dto
{
    public class AlbumDto : IEquatable<AlbumDto>
    {
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public virtual UserDto Owner { get; set; }
        public virtual PrivacySettings Privacy { get; set; }
        public virtual ICollection<PhotoDto> Photos { get; set; }
        public DateTime? MadeOn { get; set; }
        public virtual PhotoDto FrontPhoto { get; set; }

        public AlbumDto()
        {
            Photos = new List<PhotoDto>();
        }

        public bool Equals(AlbumDto other)
        {
            return this.Id == other.Id;
        }
    }
}
