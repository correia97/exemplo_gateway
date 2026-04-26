import { Component, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DragonballService } from '../../../api/dragonball.service';
import { HasRoleDirective } from '../../../auth/has-role.directive';
import { DataTableComponent } from '../../../shared/components/data-table/data-table';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state';
import type { Character, CharacterFilters } from '../../../api/types';

@Component({
  selector: 'app-character-list',
  standalone: true,
  imports: [FormsModule, DataTableComponent, PaginationComponent, EmptyStateComponent, HasRoleDirective],
  templateUrl: './character-list.html',
  styleUrl: './character-list.css',
})
export class CharacterListComponent implements OnInit {
  private dbService = inject(DragonballService);

  @Output() selectCharacter = new EventEmitter<number>();
  @Output() createCharacter = new EventEmitter<void>();

  characters: Character[] = [];
  page = 1;
  totalPages = 1;
  totalCount = 0;
  isLoading = true;
  search = '';
  raceFilter = '';
  pageSize = 10;

  columns = [
    { key: 'name', label: 'Name' },
    { key: 'race', label: 'Race' },
    { key: 'ki', label: 'Ki' },
    { key: 'transformations', label: 'Transformations', render: (c: Character) => String(c.transformations?.length ?? 0) },
  ];

  ngOnInit(): void {
    this.fetchCharacters();
  }

  fetchCharacters(): void {
    this.isLoading = true;
    const filters: CharacterFilters = { page: this.page, pageSize: this.pageSize };
    if (this.search) filters.name = this.search;
    if (this.raceFilter) filters.race = this.raceFilter;

    this.dbService.getCharacters(filters).subscribe({
      next: (result) => {
        this.characters = result.items;
        this.totalPages = result.totalPages;
        this.totalCount = result.totalCount;
        this.isLoading = false;
      },
      error: () => {
        this.characters = [];
        this.isLoading = false;
      },
    });
  }

  onSearchChange(): void {
    this.page = 1;
    this.fetchCharacters();
  }

  onRaceChange(): void {
    this.page = 1;
    this.fetchCharacters();
  }

  onPageChange(page: number): void {
    this.page = page;
    this.fetchCharacters();
  }

  keyExtractor = (c: Character) => c.id;

  onSelect(character: Character): void {
    this.selectCharacter.emit(character.id);
  }

  races = ['Saiyan', 'Human', 'Namekian', 'Frieza Race', 'Android', 'Majin', 'Angel', 'God', 'Other'];
}
