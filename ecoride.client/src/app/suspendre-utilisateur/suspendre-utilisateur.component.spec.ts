import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SuspendreUtilisateurComponent } from './suspendre-utilisateur.component';

describe('SuspendreUtilisateurComponent', () => {
  let component: SuspendreUtilisateurComponent;
  let fixture: ComponentFixture<SuspendreUtilisateurComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SuspendreUtilisateurComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SuspendreUtilisateurComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
