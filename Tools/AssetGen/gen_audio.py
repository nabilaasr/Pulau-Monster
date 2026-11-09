"""
gen_audio.py - Chiptune audio synthesizer (16-bit mono WAV, 22050 Hz).
Generates:
- 7 BGMs (20-40s loop, smooth, ending on tonic):
  BGM_Title, BGM_Village, BGM_Forest, BGM_Battle, BGM_Boss, BGM_Lake, BGM_Cave
- 13 SFX:
  SFX_Hit, SFX_Hit_Critical, SFX_Miss, SFX_LevelUp, SFX_Evolve, SFX_Victory_Jingle,
  SFX_Faint, SFX_Dialog_Blip, SFX_UI_Move, SFX_UI_Confirm, SFX_UI_Cancel,
  SFX_Save, SFX_Heal
"""

import os
import wave
import struct
import numpy as np

SAMPLE_RATE = 22050

def save_wav(path, data, sample_rate=SAMPLE_RATE):
    # Normalize to -3 dBFS (peak ~0.707 of max 32767 -> ~23168)
    max_val = np.max(np.abs(data))
    if max_val > 0:
        normalized = data * (23000.0 / max_val)
    else:
        normalized = data
    int_data = np.clip(normalized, -32767, 32767).astype(np.int16)
    
    with wave.open(path, 'w') as wf:
        wf.setnchannels(1) # mono
        wf.setsampwidth(2) # 16-bit
        wf.setframerate(sample_rate)
        wf.writeframes(int_data.tobytes())

# --- Basic Waveforms ---
def pulse_wave(freq, duration, duty=0.5, sr=SAMPLE_RATE):
    t = np.linspace(0, duration, int(sr * duration), endpoint=False)
    phase = (t * freq) % 1.0
    return np.where(phase < duty, 1.0, -1.0)

def triangle_wave(freq, duration, sr=SAMPLE_RATE):
    t = np.linspace(0, duration, int(sr * duration), endpoint=False)
    phase = (t * freq) % 1.0
    return 2.0 * np.abs(2.0 * (phase - np.floor(phase + 0.5))) - 1.0

def noise_wave(duration, sr=SAMPLE_RATE):
    length = int(sr * duration)
    return np.random.uniform(-1.0, 1.0, length)

def adsr(length, a=0.05, d=0.1, s=0.7, r=0.2):
    n_a = int(length * a)
    n_d = int(length * d)
    n_r = int(length * r)
    n_s = max(0, length - n_a - n_d - n_r)
    
    env = np.concatenate([
        np.linspace(0.0, 1.0, n_a),
        np.linspace(1.0, s, n_d),
        np.full(n_s, s),
        np.linspace(s, 0.0, n_r)
    ])
    if len(env) < length:
        env = np.pad(env, (0, length - len(env)))
    return env[:length]

# Pitch dictionary (Hz)
PITCHES = {
    'C3': 130.81, 'D3': 146.83, 'E3': 164.81, 'F3': 174.61, 'G3': 196.00, 'A3': 220.00, 'B3': 246.94,
    'C4': 261.63, 'D4': 293.66, 'E4': 329.63, 'F4': 349.23, 'G4': 392.00, 'A4': 440.00, 'B4': 493.88,
    'C5': 523.25, 'D5': 587.33, 'E5': 659.25, 'F5': 698.46, 'G5': 783.99, 'A5': 880.00, 'B5': 987.77,
    'C6': 1046.50, 'R': 0.0 # Rest
}

def synth_sequence(notes, note_dur=0.25, wave_type='pulse', duty=0.5):
    """
    notes: list of (pitch_name, dur_multiplier)
    """
    track = []
    for note, mult in notes:
        dur = note_dur * mult
        n_samples = int(SAMPLE_RATE * dur)
        if note == 'R' or PITCHES.get(note, 0) == 0:
            sound = np.zeros(n_samples)
        else:
            freq = PITCHES[note]
            if wave_type == 'pulse':
                raw = pulse_wave(freq, dur, duty=duty)
            else:
                raw = triangle_wave(freq, dur)
            env = adsr(n_samples, a=0.08, d=0.15, s=0.6, r=0.2)
            sound = raw * env
        track.append(sound)
    return np.concatenate(track)

# --- Music Themes (20-30s loops ending on tonic) ---

def make_bgm_title():
    # Energetic, heroic melody in C major
    melody = [
        ('C4', 1), ('E4', 1), ('G4', 1), ('C5', 2), ('G4', 1),
        ('A4', 1), ('B4', 1), ('C5', 2), ('G4', 2),
        ('F4', 1), ('E4', 1), ('D4', 1), ('G4', 2), ('D4', 1),
        ('E4', 1), ('F4', 1), ('E4', 1), ('D4', 1), ('C4', 3), ('R', 1),
    ] * 5 # ~26 seconds total
    lead = synth_sequence(melody, note_dur=0.2, wave_type='pulse', duty=0.25)
    bass_notes = [
        ('C3', 2), ('G3', 2), ('A3', 2), ('E3', 2),
        ('F3', 2), ('C3', 2), ('G3', 2), ('C3', 2),
    ] * 10
    bass = synth_sequence(bass_notes, note_dur=0.2, wave_type='triangle')
    min_len = min(len(lead), len(bass))
    return lead[:min_len] * 0.7 + bass[:min_len] * 0.8

def make_bgm_village():
    # Peaceful, cozy acoustic melody in G major
    melody = [
        ('G4', 2), ('B4', 1), ('D5', 2), ('B4', 1),
        ('C5', 2), ('A4', 1), ('G4', 3),
        ('E4', 2), ('G4', 1), ('D4', 2), ('G4', 1),
        ('A4', 2), ('D4', 1), ('G4', 3), ('R', 1)
    ] * 4 # ~24s
    lead = synth_sequence(melody, note_dur=0.25, wave_type='pulse', duty=0.5)
    bass_notes = [
        ('G3', 3), ('D3', 3), ('C3', 3), ('G3', 3),
        ('C3', 3), ('G3', 3), ('D3', 3), ('G3', 3)
    ] * 4
    bass = synth_sequence(bass_notes, note_dur=0.25, wave_type='triangle')
    min_len = min(len(lead), len(bass))
    return lead[:min_len] * 0.6 + bass[:min_len] * 0.7

def make_bgm_forest():
    # Mysterious, adventurous exploration in A minor
    melody = [
        ('A4', 2), ('C5', 1), ('E5', 2), ('D5', 1),
        ('C5', 1), ('B4', 1), ('A4', 2), ('E4', 2),
        ('F4', 2), ('A4', 1), ('G4', 2), ('E4', 1),
        ('F4', 1), ('E4', 1), ('D4', 1), ('E4', 1), ('A4', 3), ('R', 1)
    ] * 4 # ~26s
    lead = synth_sequence(melody, note_dur=0.25, wave_type='pulse', duty=0.25)
    bass_notes = [
        ('A3', 2), ('E3', 2), ('F3', 2), ('C3', 2),
        ('D3', 2), ('A3', 2), ('E3', 2), ('A3', 2)
    ] * 7
    bass = synth_sequence(bass_notes, note_dur=0.25, wave_type='triangle')
    min_len = min(len(lead), len(bass))
    return lead[:min_len] * 0.65 + bass[:min_len] * 0.75

def make_bgm_battle():
    # Fast, driving, tense battle theme
    melody = [
        ('D4', 1), ('D4', 1), ('F4', 1), ('A4', 1), ('G4', 1), ('F4', 1), ('E4', 1), ('C4', 1),
        ('D4', 1), ('D4', 1), ('F4', 1), ('A4', 1), ('C5', 2), ('A4', 2),
        ('B4', 1), ('G4', 1), ('A4', 1), ('F4', 1), ('G4', 1), ('E4', 1), ('F4', 1), ('D4', 1),
        ('C4', 1), ('E4', 1), ('G4', 1), ('A4', 1), ('D4', 3), ('R', 1)
    ] * 7 # ~25.2s
    lead = synth_sequence(melody, note_dur=0.15, wave_type='pulse', duty=0.25)
    bass_notes = [
        ('D3', 1), ('D3', 1), ('D3', 1), ('D3', 1),
        ('C3', 1), ('C3', 1), ('A3', 1), ('A3', 1),
    ] * 21
    bass = synth_sequence(bass_notes, note_dur=0.15, wave_type='triangle')
    min_len = min(len(lead), len(bass))
    return lead[:min_len] * 0.7 + bass[:min_len] * 0.8

def make_bgm_boss():
    # Thunderous, dark, heavy boss battle in D minor
    melody = [
        ('D4', 2), ('A4', 1), ('F4', 2), ('D4', 1),
        ('D#4', 2), ('A#4', 1), ('G4', 2), ('D#4', 1),
        ('E4', 2), ('B4', 1), ('G4', 2), ('E4', 1),
        ('D4', 1), ('F4', 1), ('A4', 1), ('D5', 2), ('A4', 1), ('D4', 3), ('R', 1)
    ] * 5 # ~25.2s
    lead = synth_sequence(melody, note_dur=0.18, wave_type='pulse', duty=0.125)
    bass_notes = [
        ('D3', 2), ('D3', 2), ('D#3', 2), ('D#3', 2),
        ('E3', 2), ('E3', 2), ('D3', 2), ('D3', 2)
    ] * 10
    bass = synth_sequence(bass_notes, note_dur=0.18, wave_type='triangle')
    min_len = min(len(lead), len(bass))
    return lead[:min_len] * 0.75 + bass[:min_len] * 0.85

def make_bgm_lake():
    # Serene, watery waltz in F major
    melody = [
        ('F4', 2), ('A4', 1), ('C5', 3),
        ('D5', 2), ('C5', 1), ('A4', 3),
        ('B4', 2), ('G4', 1), ('C5', 3),
        ('A4', 2), ('F4', 1), ('F4', 3), ('R', 1)
    ] * 4 # ~26.9s
    lead = synth_sequence(melody, note_dur=0.28, wave_type='pulse', duty=0.5)
    return lead * 0.7

def make_bgm_cave():
    # Deep, echoing, atmospheric in B minor
    melody = [
        ('B3', 3), ('D4', 2), ('F#4', 3),
        ('G4', 2), ('F#4', 2), ('D4', 3),
        ('E4', 3), ('C4', 2), ('B3', 4), ('R', 2)
    ] * 4 # ~28.8s
    lead = synth_sequence(melody, note_dur=0.3, wave_type='triangle')
    return lead * 0.8

# --- SFX ---

def make_sfx_hit():
    dur = 0.12
    n = int(SAMPLE_RATE * dur)
    t = np.linspace(0, dur, n)
    # Quick pitch drop
    freq = np.linspace(350, 80, n)
    phase = np.cumsum(freq) / SAMPLE_RATE
    wave = np.sin(2 * np.pi * phase) + noise_wave(dur) * 0.4
    env = np.linspace(1.0, 0.0, n)**1.5
    return wave * env

def make_sfx_hit_critical():
    dur = 0.22
    n = int(SAMPLE_RATE * dur)
    freq = np.linspace(700, 100, n)
    phase = np.cumsum(freq) / SAMPLE_RATE
    wave = np.sin(2 * np.pi * phase) + noise_wave(dur) * 0.7
    env = np.linspace(1.0, 0.0, n)**1.2
    return wave * env

def make_sfx_miss():
    dur = 0.15
    n = int(SAMPLE_RATE * dur)
    freq = np.linspace(600, 300, n)
    phase = np.cumsum(freq) / SAMPLE_RATE
    wave = triangle_wave(400, dur) * 0.3 + noise_wave(dur) * 0.7
    env = np.sin(np.pi * np.linspace(0, 1, n))
    return wave * env

def make_sfx_level_up():
    notes = [('C4', 1), ('E4', 1), ('G4', 1), ('C5', 1), ('E5', 1), ('G5', 2)]
    return synth_sequence(notes, note_dur=0.09, wave_type='pulse', duty=0.25)

def make_sfx_evolve():
    notes = [('C4', 1), ('D4', 1), ('E4', 1), ('F4', 1), ('G4', 1), ('A4', 1), ('B4', 1), ('C5', 2), ('E5', 2), ('C6', 3)]
    return synth_sequence(notes, note_dur=0.07, wave_type='pulse', duty=0.5)

def make_sfx_victory():
    # Triumphant fanfare ~2.8s
    notes = [
        ('C4', 1), ('C4', 1), ('C4', 1), ('C4', 2),
        ('G#3', 2), ('A#3', 2),
        ('C4', 2), ('A#3', 1), ('C4', 4)
    ]
    return synth_sequence(notes, note_dur=0.2, wave_type='pulse', duty=0.25)

def make_sfx_faint():
    dur = 0.45
    n = int(SAMPLE_RATE * dur)
    freq = np.linspace(400, 80, n)
    phase = np.cumsum(freq) / SAMPLE_RATE
    wave = np.sin(2 * np.pi * phase)
    env = np.linspace(1.0, 0.0, n)**2
    return wave * env

def make_sfx_dialog_blip():
    # Very short ~30ms
    dur = 0.03
    n = int(SAMPLE_RATE * dur)
    wave = pulse_wave(580, dur, duty=0.25)
    env = np.linspace(1.0, 0.0, n)
    return wave * env

def make_sfx_ui_move():
    dur = 0.04
    n = int(SAMPLE_RATE * dur)
    wave = pulse_wave(700, dur, duty=0.5)
    env = np.linspace(0.8, 0.0, n)
    return wave * env

def make_sfx_ui_confirm():
    notes = [('E5', 1), ('B5', 2)]
    return synth_sequence(notes, note_dur=0.06, wave_type='pulse', duty=0.25)

def make_sfx_ui_cancel():
    notes = [('B4', 1), ('F4', 2)]
    return synth_sequence(notes, note_dur=0.06, wave_type='pulse', duty=0.5)

def make_sfx_save():
    # Sparkle chime
    notes = [('C5', 1), ('G5', 1), ('C6', 1), ('E6', 2)]
    return synth_sequence(notes, note_dur=0.08, wave_type='pulse', duty=0.25)

def make_sfx_heal():
    # Warm rising chime
    notes = [('C4', 1), ('E4', 1), ('G4', 1), ('C5', 2)]
    return synth_sequence(notes, note_dur=0.1, wave_type='triangle')

def generate_all_audio(out_dir):
    os.makedirs(out_dir, exist_ok=True)
    audio_map = {
        # BGM
        "BGM_Title.wav": make_bgm_title(),
        "BGM_Village.wav": make_bgm_village(),
        "BGM_Forest.wav": make_bgm_forest(),
        "BGM_Battle.wav": make_bgm_battle(),
        "BGM_Boss.wav": make_bgm_boss(),
        "BGM_Lake.wav": make_bgm_lake(),
        "BGM_Cave.wav": make_bgm_cave(),
        # SFX
        "SFX_Hit.wav": make_sfx_hit(),
        "SFX_Hit_Critical.wav": make_sfx_hit_critical(),
        "SFX_Miss.wav": make_sfx_miss(),
        "SFX_LevelUp.wav": make_sfx_level_up(),
        "SFX_Evolve.wav": make_sfx_evolve(),
        "SFX_Victory_Jingle.wav": make_sfx_victory(),
        "SFX_Faint.wav": make_sfx_faint(),
        "SFX_Dialog_Blip.wav": make_sfx_dialog_blip(),
        "SFX_UI_Move.wav": make_sfx_ui_move(),
        "SFX_UI_Confirm.wav": make_sfx_ui_confirm(),
        "SFX_UI_Cancel.wav": make_sfx_ui_cancel(),
        "SFX_Save.wav": make_sfx_save(),
        "SFX_Heal.wav": make_sfx_heal(),
    }
    results = {}
    for filename, sound_data in audio_map.items():
        path = os.path.join(out_dir, filename)
        save_wav(path, sound_data)
        results[filename] = path
    return results

if __name__ == "__main__":
    generate_all_audio("temp_audio")
    print("Audio generated.")
