import { Component, Input, Output, EventEmitter } from '@angular/core';
import { HasRoleDirective } from '../../../auth/has-role.directive';
import type { Character } from '../../../api/types';

@Component({
  selector: 'app-character-detail',
  standalone: true,
  imports: [HasRoleDirective],
  templateUrl: './character-detail.html',
  styleUrl: './character-detail.css',
})
export class CharacterDetailComponent {
  @Input({ required: true }) character!: Character;
  @Output() edit = new EventEmitter<void>();
  @Output() delete = new EventEmitter<void>();
  @Output() back = new EventEmitter<void>();
}
