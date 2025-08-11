import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ItinerairevueComponent } from './itinerairevue.component';

describe('ItinerairevueComponent', () => {
  let component: ItinerairevueComponent;
  let fixture: ComponentFixture<ItinerairevueComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ItinerairevueComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ItinerairevueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
