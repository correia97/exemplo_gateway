import { Component, inject } from '@angular/core';
import { AuthService } from '../../../auth/auth.service';
import { DragonballService } from '../../../api/dragonball.service';
import { CharacterListComponent } from '../character-list/character-list';
import { CharacterDetailComponent } from '../character-detail/character-detail';
import { CharacterFormComponent } from '../character-form/character-form';
import { ErrorDisplayComponent } from '../../../shared/components/error-display/error-display';
import { ToastService } from '../../../shared/services/toast.service';
import type { Character, CharacterCreatePayload } from '../../../api/types';

export type CharacterView = 'list' | 'detail' | 'create' | 'edit';

@Component({
  selector: 'app-dragonball-page',
  standalone: true,
  imports: [CharacterListComponent, CharacterDetailComponent, CharacterFormComponent, ErrorDisplayComponent],
  templateUrl: './dragonball-page.html',
  styleUrl: './dragonball-page.css',
})
export class DragonballPageComponent {
  private dbService = inject(DragonballService);
  private auth = inject(AuthService);
  private toast = inject(ToastService);

  view: CharacterView = 'list';
  selectedCharacter: Character | null = null;
  isAuthenticated$ = this.auth.isAuthenticated$;

  showList(): void {
    this.view = 'list';
    this.selectedCharacter = null;
  }

  showDetail(id: number): void {
    this.dbService.getCharacter(id).subscribe({
      next: (c) => { this.selectedCharacter = c; this.view = 'detail'; },
      error: (err) => this.toast.showError(err.message || 'Failed to load character'),
    });
  }

  showCreate(): void {
    this.view = 'create';
  }

  showEdit(): void {
    this.view = 'edit';
  }

  handleCreate(data: CharacterCreatePayload): void {
    this.dbService.createCharacter(data).subscribe({
      next: () => this.showList(),
      error: (err) => this.toast.showError(err.message || 'Failed to create character'),
    });
  }

  handleUpdate(data: Partial<CharacterCreatePayload>): void {
    if (!this.selectedCharacter) return;
    this.dbService.updateCharacter(this.selectedCharacter.id, data).subscribe({
      next: (updated) => { this.selectedCharacter = updated; this.view = 'detail'; },
      error: (err) => this.toast.showError(err.message || 'Failed to update character'),
    });
  }

  handleDelete(): void {
    if (!this.selectedCharacter) return;
    if (!confirm('Delete this character?')) return;
    this.dbService.deleteCharacter(this.selectedCharacter.id).subscribe({
      next: () => this.showList(),
      error: (err) => this.toast.showError(err.message || 'Failed to delete character'),
    });
  }
}
