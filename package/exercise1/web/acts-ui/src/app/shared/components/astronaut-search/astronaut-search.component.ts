import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Client, AstronautDuty, PersonAstronaut, CreateAstronautDuty, UpdateAstronautDuty, GetPeopleResult, GetAstronautDutiesByNameResult, BaseResponse } from '../../../services/api-client';

@Component({
  selector: 'app-astronaut-search',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
  ],
  templateUrl: './astronaut-search.component.html',
  styleUrl: './astronaut-search.component.scss',
})
export class AstronautSearchComponent implements OnInit {
  allPeople = signal<PersonAstronaut[]>([]);
  filteredPeople = signal<PersonAstronaut[]>([]);
  searchQuery = signal('');
  expandedPersonId = signal<number | null>(null);
  personDuties = signal<Map<number, AstronautDuty[]>>(new Map());
  isLoading = signal(false);
  isLoadingDuties = signal(false);
  errorMessage = signal('');
  successMessage = signal('');
  showCreatePersonModal = signal(false);
  newPersonName = signal('');
  isCreatingPerson = signal(false);

  showCreateDutyModal = signal(false);
  selectedPersonForDuty = signal<PersonAstronaut | null>(null);
  dutyForm = signal({
    rank: '',
    dutyTitle: '',
    dutyStartDate: '',
  });
  isCreatingDuty = signal(false);

  showEditDutyModal = signal(false);
  selectedDutyForEdit = signal<AstronautDuty | null>(null);
  editDutyForm = signal({
    rank: '',
    dutyTitle: '',
    dutyStartDate: '',
    dutyEndDate: '',
  });
  isUpdatingDuty = signal(false);

  constructor(private apiClient: Client) {}

  ngOnInit(): void {
    this.loadAllPeople();
  }

  loadAllPeople(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.apiClient.getAllPeople().subscribe({
      next: (response: GetPeopleResult) => {
        if (response.success && response.people) {
          this.allPeople.set(response.people);
          this.applySearchFilter();
        } else {
          this.errorMessage.set(response.message || 'No people found');
          this.allPeople.set([]);
          this.filteredPeople.set([]);
        }
        this.isLoading.set(false);
      },
      error: (error: any) => {
        this.errorMessage.set('Failed to fetch people');
        this.allPeople.set([]);
        this.filteredPeople.set([]);
        this.isLoading.set(false);
      },
    });
  }

  applySearchFilter(): void {
    const query = this.searchQuery().toLowerCase().trim();

    if (!query) {
      const sorted = [...this.allPeople()].sort((a, b) =>
        (a.name || '').localeCompare(b.name || '')
      );
      this.filteredPeople.set(sorted);
      this.successMessage.set(`Showing all ${this.allPeople().length} astronaut(s)`);
    } else {
      const filtered = this.allPeople().filter(person =>
        person.name?.toLowerCase().includes(query)
      );
      const sorted = filtered.sort((a, b) =>
        (a.name || '').localeCompare(b.name || '')
      );
      this.filteredPeople.set(sorted);
      this.successMessage.set(`Found ${filtered.length} matching astronaut(s)`);
    }
  }

  onSearchChange(value: string): void {
    this.searchQuery.set(value);
    this.applySearchFilter();
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.applySearchFilter();
  }

  togglePerson(person: PersonAstronaut): void {
    const personId = person.personId ?? null;

    if (this.expandedPersonId() === personId) {
      // Collapse if already expanded
      this.expandedPersonId.set(null);
    } else {
      // Expand and load duties if not already loaded
      this.expandedPersonId.set(personId);

      if (personId && !this.personDuties().has(personId) && person.name) {
        this.loadDutiesForPerson(person.name, personId);
      }
    }
  }

  loadDutiesForPerson(name: string, personId: number): void {
    this.isLoadingDuties.set(true);

    this.apiClient.getAstronautDutiesByName(name).subscribe({
      next: (response: GetAstronautDutiesByNameResult) => {
        if (response.success && response.astronautDuties) {
          const updatedMap = new Map(this.personDuties());
          updatedMap.set(personId, response.astronautDuties);
          this.personDuties.set(updatedMap);
        }
        this.isLoadingDuties.set(false);
      },
      error: (error: any) => {
        this.isLoadingDuties.set(false);
      },
    });
  }

  isExpanded(personId: number | undefined): boolean {
    return this.expandedPersonId() === personId;
  }

  getDutiesForPerson(personId: number | undefined): AstronautDuty[] {
    if (!personId) return [];
    return this.personDuties().get(personId) || [];
  }

  formatDate(date: Date | undefined): string {
    if (!date) return 'N/A';
    try {
      return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
      });
    } catch {
      return 'Invalid Date';
    }
  }

  openCreatePersonModal(): void {
    this.showCreatePersonModal.set(true);
    this.newPersonName.set('');
    this.errorMessage.set('');
  }

  closeCreatePersonModal(): void {
    this.showCreatePersonModal.set(false);
    this.newPersonName.set('');
  }

  createPerson(): void {
    if (!this.newPersonName().trim()) {
      this.errorMessage.set('Please enter a person name');
      return;
    }

    this.isCreatingPerson.set(true);
    this.errorMessage.set('');

    this.apiClient.createPerson(this.newPersonName()).subscribe({
      next: (response: BaseResponse) => {
        if (response.success) {
          this.successMessage.set(`Successfully created: ${this.newPersonName()}`);
          this.closeCreatePersonModal();
          this.loadAllPeople();
        } else {
          this.errorMessage.set(response.message || 'Failed to create person');
        }
        this.isCreatingPerson.set(false);
      },
      error: (error: any) => {
        this.errorMessage.set('Failed to create person');
        this.isCreatingPerson.set(false);
      },
    });
  }

  openCreateDutyModal(person: PersonAstronaut, event: Event): void {
    event.stopPropagation();
    this.selectedPersonForDuty.set(person);
    this.showCreateDutyModal.set(true);
    this.dutyForm.set({
      rank: '',
      dutyTitle: '',
      dutyStartDate: '',
    });
    this.errorMessage.set('');
  }

  closeCreateDutyModal(): void {
    this.showCreateDutyModal.set(false);
    this.selectedPersonForDuty.set(null);
    this.dutyForm.set({
      rank: '',
      dutyTitle: '',
      dutyStartDate: '',
    });
  }

  updateDutyField(field: string, value: string): void {
    const current = this.dutyForm();
    this.dutyForm.set({ ...current, [field]: value });
  }

  createDuty(): void {
    const formData = this.dutyForm();
    const person = this.selectedPersonForDuty();

    if (!person || !formData.rank.trim() || !formData.dutyTitle.trim() || !formData.dutyStartDate) {
      this.errorMessage.set('Please fill in all fields');
      return;
    }

    this.isCreatingDuty.set(true);
    this.errorMessage.set('');

    const request = new CreateAstronautDuty({
      name: person.name,
      rank: formData.rank,
      dutyTitle: formData.dutyTitle,
      dutyStartDate: new Date(formData.dutyStartDate),
    });

    this.apiClient.createAstronautDuty(request).subscribe({
      next: (response: BaseResponse) => {
        if (response.success) {
          this.successMessage.set(`Successfully created duty for ${person.name}`);
          this.closeCreateDutyModal();

          // Reload duties for this person
          if (person.personId && person.name) {
            this.loadDutiesForPerson(person.name, person.personId);
          }

          // Reload all people to get updated career data
          this.loadAllPeople();
        } else {
          this.errorMessage.set(response.message || 'Failed to create duty');
        }
        this.isCreatingDuty.set(false);
      },
      error: (error: any) => {
        this.errorMessage.set('Failed to create astronaut duty');
        this.isCreatingDuty.set(false);
      },
    });
  }

  openEditDutyModal(duty: AstronautDuty, event: Event): void {
    event.stopPropagation();
    this.selectedDutyForEdit.set(duty);
    this.showEditDutyModal.set(true);

    // Format dates for input fields
    const startDate = duty.dutyStartDate ? new Date(duty.dutyStartDate).toISOString().split('T')[0] : '';
    const endDate = duty.dutyEndDate ? new Date(duty.dutyEndDate).toISOString().split('T')[0] : '';

    this.editDutyForm.set({
      rank: duty.rank || '',
      dutyTitle: duty.dutyTitle || '',
      dutyStartDate: startDate,
      dutyEndDate: endDate,
    });
    this.errorMessage.set('');
  }

  closeEditDutyModal(): void {
    this.showEditDutyModal.set(false);
    this.selectedDutyForEdit.set(null);
    this.editDutyForm.set({
      rank: '',
      dutyTitle: '',
      dutyStartDate: '',
      dutyEndDate: '',
    });
  }

  updateEditDutyField(field: string, value: string): void {
    const current = this.editDutyForm();
    this.editDutyForm.set({ ...current, [field]: value });
  }

  updateDuty(): void {
    const formData = this.editDutyForm();
    const duty = this.selectedDutyForEdit();

    if (!duty || !formData.rank.trim() || !formData.dutyTitle.trim() || !formData.dutyStartDate) {
      this.errorMessage.set('Please fill in all required fields');
      return;
    }

    this.isUpdatingDuty.set(true);
    this.errorMessage.set('');

    const request = new UpdateAstronautDuty({
      id: duty.id,
      rank: formData.rank,
      dutyTitle: formData.dutyTitle,
      dutyStartDate: new Date(formData.dutyStartDate),
      dutyEndDate: formData.dutyEndDate ? new Date(formData.dutyEndDate) : undefined,
    });

    this.apiClient.updateAstronautDuty(duty.id!, request).subscribe({
      next: (response: BaseResponse) => {
        if (response.success) {
          this.successMessage.set(`Successfully updated duty`);
          this.closeEditDutyModal();

          // Reload duties for the person associated with this duty
          const personId = duty.personId;
          const personName = this.allPeople().find(p => p.personId === personId)?.name;

          if (personId && personName) {
            this.loadDutiesForPerson(personName, personId);
          }

          // Reload all people to get updated career data
          this.loadAllPeople();
        } else {
          this.errorMessage.set(response.message || 'Failed to update duty');
        }
        this.isUpdatingDuty.set(false);
      },
      error: (error: any) => {
        this.errorMessage.set('Failed to update astronaut duty');
        this.isUpdatingDuty.set(false);
      },
    });
  }
}
