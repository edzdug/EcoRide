import { Component, OnInit } from '@angular/core';
import { FormDataService } from '../itineraireform/itineraireform.component';

@Component({
  selector: 'app-itinerairevue',
  standalone: false,
  templateUrl: './itinerairevue.component.html',
  styleUrl: './itinerairevue.component.css'
})
export class ItinerairevueComponent implements OnInit {

  form: any;

  constructor(private formDataService: FormDataService) { }

  ngOnInit() {
    this.form = this.formDataService.getForm();
  }
}
