using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace WindowsGame2 {

    enum Sound {
        MouseOver = 0,
        Shooting,
        GunLoading,
        PlayerHurt,
        NewWave,
        WaveOver,
        UpgradeSuccess,
        UpgradeFail
    }

    enum Music {
        MainMenu = 0,
        DeathSong,
        GameMusic
    }

    class SoundManager {
        
        private List<SoundEffect> soundEffectList = new List<SoundEffect>();
        private List<Song> musicList = new List<Song>();

        public bool soundFxEnabled = true;
        public bool musicEnabled = true;

        private string[] soundEffectLocations = {
            "Sounds/scroll_over",
            "Sounds/shooting",
            "Sounds/gun_loading",
            "Sounds/hurt",
            "Sounds/new_wave",
            "Sounds/wave_over",
            "Sounds/upgrade_success",
            "Sounds/upgrade_fail"
        };

        private string[] musicLocations = {
            "Sounds/MainMenu",
            "Sounds/death_song",
            "Sounds/game_music"
        };

        private static SoundManager instance = null;

        private SoundManager() { }

        public static SoundManager getInstance() {
            if (instance == null) {
                instance = new SoundManager();
                MediaPlayer.IsRepeating = true;
                SoundEffect.MasterVolume = 0.5f;
            }

            return instance;
        }

        public void LoadContent(ContentManager content) {
            foreach (string s in soundEffectLocations) {
                soundEffectList.Add(content.Load<SoundEffect>(s));
            }

            foreach (string s in musicLocations) {
                musicList.Add(content.Load<Song>(s));
            }
        }

        public void playSound(Sound sound) {
            if (soundFxEnabled) {
                soundEffectList[(int)sound].Play();
            }
        }

        public void playMusic(Music music) {
            if (musicEnabled) {
                MediaPlayer.Play(musicList[(int)music]);
            }
        }
    }
}
