import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EspaceEmployeComponent } from './espace-employe.component';

describe('EspaceEmployeComponent', () => {
  let component: EspaceEmployeComponent;
  let fixture: ComponentFixture<EspaceEmployeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EspaceEmployeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EspaceEmployeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
