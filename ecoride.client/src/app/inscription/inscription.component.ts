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
    if (this.form.photo) {
      const base64Photo = await blobToBase64(this.form.photo);
      const utilisateurToSend = { ...this.form, photo: base64Photo };
      this.http.post('/api/utilisateur/PostUser', utilisateurToSend).subscribe(response => {
        console.log("Utilisateur créé :", response);
      });
    } else {
      this.http.post('/api/utilisateur/PostUser', this.form).subscribe(response => {
        console.log("Utilisateur créé :", response);
      });
    }
  }


  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.form.photo = file;
    }
  }


}
function blobToBase64(blob: Blob): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onerror = reject;
    reader.onload = () => {
      resolve(reader.result as string); // contient le base64
    };
    reader.readAsDataURL(blob); // convertit le blob en data URI base64
  });
}


