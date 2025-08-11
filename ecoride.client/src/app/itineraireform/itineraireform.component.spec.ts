import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ItineraireformComponent } from './itineraireform.component';

describe('ItineraireformComponent', () => {
  let component: ItineraireformComponent;
  let fixture: ComponentFixture<ItineraireformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ItineraireformComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ItineraireformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
