import { Component, Injectable } from '@angular/core';
import { Router } from '@angular/router';

type Form = {
  depart: string;
  destination: string;
  date: Date;
};

@Injectable({ providedIn: 'root' })
export class FormDataService {
  private _form: Form | null = null;

  setForm(form: Form) {
    this._form = form;
  }

  getForm(): Form | null {
    return this._form;
  }
}

@Component({
  selector: 'app-itineraireform',
  standalone: false,
  templateUrl: './itineraireform.component.html',
  styleUrls: ['./itineraireform.component.css']
})
export class ItineraireformComponent {

  form: Form = {
    depart: 'Paris',
    destination: 'Reims',
    date: new Date()
  };

  constructor(
    private router: Router,                      
    private formDataService: FormDataService     
  ) { }

  async envoie() {
    this.formDataService.setForm(this.form);     
    this.router.navigate(['/itinerairevue']);         
  }

  

}
