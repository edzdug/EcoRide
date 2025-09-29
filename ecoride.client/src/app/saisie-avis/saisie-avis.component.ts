import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService} from '../authentification/auth.service'

@Component({
  selector: 'app-saisie-avis',
  standalone: false,
  templateUrl: './saisie-avis.component.html',
  styleUrl: './saisie-avis.component.css'
})
export class SaisieAvisComponent {

  user: any;
  utilisateurId!: number;
  covoiturageId!: number;
  avis = {
    note: '',
    commentaire: ''
  };

  constructor(private http: HttpClient, private route: ActivatedRoute, private router : Router,private authService: AuthService) { }

  ngOnInit() {
    this.covoiturageId = Number(this.route.snapshot.paramMap.get('id'));
    this.user = this.authService.currentUserValue;

    if (this.user) {
      this.utilisateurId = this.user.id;
      console.log('Utilisateur connecté :', this.utilisateurId);
    } else {
      console.error('Utilisateur non connecté ou ID manquant');
      this.router.navigate(['/login']);
    }
  }

  submit() {
    if (!this.avis.note) {
      alert('Veuillez sélectionner une note.');
      return;
    }

    const avisDto = {
      note: this.avis.note,
      commentaire: this.avis.commentaire
    };

    this.http.post(`/api/Utilisateur/Avis/Envoyer/${this.utilisateurId}/${this.covoiturageId}`, avisDto)
      .subscribe({
        next: () => {
          alert('Avis envoyé avec succès !');
        },
        error: (err) => {
          console.error('Erreur lors de l\'envoi de l\'avis :', err);
          alert('Erreur lors de l\'envoi.');
        }
      });
  }


}
