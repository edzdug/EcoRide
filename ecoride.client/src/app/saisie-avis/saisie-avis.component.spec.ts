import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SaisieAvisComponent } from './saisie-avis.component';

describe('SaisieAvisComponent', () => {
  let component: SaisieAvisComponent;
  let fixture: ComponentFixture<SaisieAvisComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SaisieAvisComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SaisieAvisComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
