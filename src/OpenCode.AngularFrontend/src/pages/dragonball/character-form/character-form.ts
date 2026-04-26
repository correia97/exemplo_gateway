import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import type { CharacterCreatePayload, Character } from '../../../api/types';

@Component({
  selector: 'app-character-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './character-form.html',
  styleUrl: './character-form.css',
})
export class CharacterFormComponent implements OnInit {
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() initial?: Character;
  @Output() save = new EventEmitter<CharacterCreatePayload>();
  @Output() cancel = new EventEmitter<void>();

  name = '';
  race = '';
  ki = '';
  maxKi = '';
  description = '';
  planetId: number | undefined;
  submitting = false;

  ngOnInit(): void {
    if (this.initial) {
      this.name = this.initial.name;
      this.race = this.initial.race;
      this.ki = this.initial.ki;
      this.maxKi = this.initial.maxKi;
      this.description = this.initial.description ?? '';
      this.planetId = this.initial.planet?.id;
    }
  }

  onSubmit(): void {
    if (!this.name.trim() || !this.race.trim() || !this.ki.trim()) return;
    this.submitting = true;
    this.save.emit({
      name: this.name.trim(),
      race: this.race.trim(),
      ki: this.ki.trim(),
      maxKi: this.maxKi.trim() || undefined,
      description: this.description.trim() || undefined,
      planetId: this.planetId,
    });
  }
}
