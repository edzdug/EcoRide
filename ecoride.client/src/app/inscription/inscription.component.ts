import { HttpClient } from '@angular/common/http';
import { Component, Injectable } from '@angular/core';
import { Router } from '@angular/router';

type Form = {
  nom: string;
  prenom: string;
  email: string;
  password: string;
  telephone: string;
  adresse: string;
  dateNaissance: string;
  photo: Blob | undefined;
  pseudo: string;
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
  selector: 'app-inscription',
  standalone: false,
  templateUrl: './inscription.component.html',
  styleUrl: './inscription.component.css'
})
export class InscriptionComponent {
  form: Form = {
    nom: "emma",
    prenom: "smith",
    email: "emma@sfr.fr",
    password: "Mot De Passe",
    telephone: "06299458234",
    adresse: "91 route nationale, Reims",
    dateNaissance: "12 octobre 1990",
    photo: undefined,
    pseudo: "emi"
  };

  constructor(
    private http: HttpClient,
    private router: Router,
    private formDataService: FormDataService
  ) { }

  async envoie() {
    this.formDataService.setForm(this.form);
    this.router.navigate(['/resultat']);
  }
}


