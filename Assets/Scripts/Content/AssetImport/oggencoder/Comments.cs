using System.Collections.Generic;
using System.Text;

namespace OggVorbisEncoder
{
    public class Comments
    {
        private readonly List<string> _userComments = new List<string>();

        public List<string> UserComments { get { return _userComments; } }

        public void AddTag(string tag, string contents)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(tag);
            stringBuilder.Append("=");
            stringBuilder.Append(contents);
            _userComments.Add(stringBuilder.ToString());
        }
    }
}