import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SaisieCovoiturageComponent } from './saisie-covoiturage.component';

describe('SaisieCovoiturageComponent', () => {
  let component: SaisieCovoiturageComponent;
  let fixture: ComponentFixture<SaisieCovoiturageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SaisieCovoiturageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SaisieCovoiturageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
