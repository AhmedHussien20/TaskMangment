import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

interface TaskComment {
  id: number;
  sender: string;
  date: string;
  timeLabel: string;
  comment: string;
  ip: string;
}

@Component({
  selector: 'app-task-comments',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './task-comments.component.html'
})
export class TaskCommentsComponent implements OnInit {

  @Input() taskId!: number;

  comments: TaskComment[] = [];

  ngOnInit(): void {
    this.loadComments();
  }

  loadComments(): void {
    //   Replace with API
    this.comments = [
      {
        id: 1,
        sender: 'المهندس وسام أبو خضر',
        date: '2025-11-11T14:56:00',
        timeLabel: 'PM 2:56',
        ip: '149.109.142.189',
        comment:
          'أسامة أبو وشاح – تم التواصل اليوم مع أسامة بخصوص تحديد موعد اجتماع بالرياض...'
      },
      {
        id: 2,
        sender: 'المهندس وسام أبو خضر',
        date: '2025-11-12T09:54:00',
        timeLabel: 'AM 9:54',
        ip: '149.109.142.189',
        comment:
          'تم الرد على الإيميل من خلال أسامة بأنه متواجد بالرياض يوم 11/24، 23...'
      }
    ];
  }

  timeAgo(date: string): string {
    const diff = Date.now() - new Date(date).getTime();
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);

    if (days > 0) return `منذ ${days} يوم`;
    if (hours > 0) return `منذ ${hours} ساعة`;
    return `منذ ${minutes} دقيقة`;
  }
}
