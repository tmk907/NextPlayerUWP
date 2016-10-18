using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWP.Common
{
    public interface IPlaybackService
    {
        void ApplyRepeatState();
        void ChangeShuffle();
        void ResetPlaybackRate();
        void ResetBalance();


        void FastForward();
        void Rewind();
        void Play();
        void Pause();
        void TogglePlayPause();
        void Next();
        void Previous();
        void JumpTo();
        void PlayNewList();

    }

    public class PlaybackServiceBase
    {

    }
}
