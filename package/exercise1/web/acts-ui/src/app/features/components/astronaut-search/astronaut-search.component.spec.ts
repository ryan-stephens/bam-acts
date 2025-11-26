import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AstronautSearchComponent } from './astronaut-search.component';
import { ApiService } from '../../../services/api.service';
import { of, throwError } from 'rxjs';

describe('AstronautSearchComponent', () => {
  let component: AstronautSearchComponent;
  let fixture: ComponentFixture<AstronautSearchComponent>;
  let apiService: jasmine.SpyObj<ApiService>;

  beforeEach(async () => {
    const apiServiceSpy = jasmine.createSpyObj('ApiService', [
      'getAstronautDutiesByName',
    ]);

    await TestBed.configureTestingModule({
      imports: [AstronautSearchComponent],
      providers: [{ provide: ApiService, useValue: apiServiceSpy }],
    }).compileComponents();

    apiService = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
    fixture = TestBed.createComponent(AstronautSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show error when search name is empty', () => {
    component.searchAstronautDuties();
    expect(component.errorMessage()).toContain('Please enter');
  });

  it('should fetch astronaut duties on search', () => {
    const mockDuties = [
      {
        id: '1',
        personName: 'John Doe',
        rank: 'Colonel',
        title: 'Astronaut',
        startDate: '2020-01-01',
        endDate: undefined,
        isCurrent: true,
      },
    ];

    apiService.getAstronautDutiesByName.and.returnValue(
      of({ success: true, message: 'Success', responseCode: 200, data: mockDuties })
    );

    component.searchName.set('John Doe');
    component.searchAstronautDuties();

    expect(apiService.getAstronautDutiesByName).toHaveBeenCalledWith('John Doe');
    expect(component.duties()).toEqual(mockDuties);
  });

  it('should clear search on clearSearch', () => {
    component.searchName.set('John Doe');
    component.duties.set([
      {
        id: '1',
        personName: 'John Doe',
        rank: 'Colonel',
        title: 'Astronaut',
        startDate: '2020-01-01',
        endDate: undefined,
        isCurrent: true,
      },
    ]);

    component.clearSearch();

    expect(component.searchName()).toBe('');
    expect(component.duties()).toEqual([]);
  });

  it('should format date correctly', () => {
    const result = component.formatDate('2020-01-15');
    expect(result).toContain('Jan');
    expect(result).toContain('15');
    expect(result).toContain('2020');
  });

  it('should handle API error', () => {
    apiService.getAstronautDutiesByName.and.returnValue(
      throwError(() => ({ error: { message: 'API Error' } }))
    );

    component.searchName.set('John Doe');
    component.searchAstronautDuties();

    expect(component.errorMessage()).toContain('Failed');
    expect(component.duties()).toEqual([]);
  });
});
