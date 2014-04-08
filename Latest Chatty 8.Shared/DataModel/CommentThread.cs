﻿using Latest_Chatty_8.Shared;
using Latest_Chatty_8.Shared.Settings;
using Latest_Chatty_8.Shared.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Latest_Chatty_8.DataModel
{
	public class CommentThread : BindableBase
	{
		private int npcId = 0;
		/// <summary>
		/// Comment ID
		/// </summary>
		[DataMember]
		public int Id
		{
			get { return npcId; }
			set { this.SetProperty(ref this.npcId, value); }
		}
		
		private int npcReplyCount;
		/// <summary>
		/// Count of replies to this comment
		/// </summary>
		[DataMember]
		public int ReplyCount
		{
			get { return this.npcReplyCount; }
			set { this.SetProperty(ref this.npcReplyCount, value); }
		}

		private bool npcUserParticipated = false;
		/// <summary>
		/// Indicates whether the currently logged in user has participated in this thread or not
		/// </summary>
		[DataMember]
		public bool UserParticipated
		{
			get { return npcUserParticipated; }
			set { this.SetProperty(ref this.npcUserParticipated, value); }
		}

		private bool npcHasNewReplies = false;
		/// <summary>
		/// Indicates if this comment has new replies since the last time it was loaded
		/// </summary>
		[DataMember]
		public bool HasNewReplies
		{
			get { return npcHasNewReplies; }
			set { this.SetProperty(ref this.npcHasNewReplies, value); }
		}

		private PostCategory npcCategory = PostCategory.ontopic;
		/// <summary>
		/// Comment category - NWS, Political, etc.
		/// </summary>
		[DataMember]
		public PostCategory Category
		{
			get { return npcCategory; }
			set { this.SetProperty(ref this.npcCategory, value); }
		}

		private string npcAuthor = string.Empty;
		/// <summary>
		/// Comment author username
		/// </summary>
		[DataMember]
		public string Author
		{
			get { return npcAuthor; }
			set { this.SetProperty(ref this.npcAuthor, value); }
		}

		private string npcDateText = string.Empty;
		/// <summary>
		/// Date posted as a string
		/// </summary>
		[DataMember]
		public string DateText
		{
			get { return npcDateText; }
			set { this.SetProperty(ref this.npcDateText, value); }
		}

		private string npcPreview = string.Empty;
		/// <summary>
		/// Preview text
		/// </summary>
		[DataMember]
		public string Preview
		{
			get { return npcPreview; }
			set { this.SetProperty(ref this.npcPreview, value); }
		}

		private bool npcIsPinned = false;
		/// <summary>
		/// Indicates if this comment is pinned or not
		/// </summary>
		[DataMember]
		public bool IsPinned
		{
			get { return npcIsPinned; }
			set { this.SetProperty(ref this.npcIsPinned, value); }
		}

		private ObservableCollection<Comment> comments;
		public ReadOnlyObservableCollection<Comment> Comments;

		private bool npcIsCollapsed = false;
		/// <summary>
		/// Indicates if this comment is collapsed or not
		/// </summary>
		[DataMember]
		public bool IsCollapsed
		{
			get { return npcIsCollapsed; }
			set { this.SetProperty(ref this.npcIsCollapsed, value); }
		}

		private DateTime npcDate;
		/// <summary>
		/// Gets or sets the date.
		[DataMember]
		public DateTime Date
		{
			get { return npcDate; }
			set { this.SetProperty(ref this.npcDate, value); }
		}

		public bool IsExpired
		{
			//TODO: This isn't quite right.  It should be based on the root comment, not the current post time.
			get { return (this.Date.AddHours(18).ToUniversalTime() < DateTime.UtcNow); }
		}

		public CommentThread(Comment rootComment)
		{
			this.comments = new ObservableCollection<Comment>();
			this.Comments = new ReadOnlyObservableCollection<Comment>(this.comments);

			this.Id = rootComment.Id;
			this.Date = rootComment.Date;
			this.Category = rootComment.Category;
			if (rootComment.UserIsAuthor) { this.UserParticipated = true; }
			this.ReplyCount = 1;
			this.Author = rootComment.Author;
			this.DateText = rootComment.DateText;
			this.Preview = rootComment.Preview;

			this.comments.Add(rootComment);

			this.CollapseIfRequired();
		}

		public void AddReply(Comment c)
		{
			//Can't directly add a parent comment.
			if (c.ParentId == 0) return;

			Comment insertAfter = null;
			var repliesToParent = this.comments.Where(c1 => c1.ParentId == c.ParentId);
			if(repliesToParent.Any())
			{
				//If there are replies, we need to figure out where we fit in.
				var lastReplyBeforeUs = repliesToParent.OrderBy(r => r.Id).LastOrDefault(r => r.Id < c.Id); //Find the last reply that should come before this one.
				if(lastReplyBeforeUs != null)
				{
					insertAfter = FindLastCommentInChain(lastReplyBeforeUs); //Now we look at all the replies to this comment, if any.  Find the last one of those.  That's where we need to insert ourselves.
				}
				else
				{
					//If there are no comments that come before this one, we find the parent comment and insert ourselves right after it.
					insertAfter = this.comments.SingleOrDefault(p => p.Id == c.ParentId);
				}
			}
			else
			{
				//If there aren't any replies to the parent of this post, we're the first one.  We'll just stick ourselves at the end.
				insertAfter = this.comments.SingleOrDefault(p => p.Id == c.ParentId);
			}
			if(insertAfter != null)
			{
				var location = this.comments.IndexOf(insertAfter);
				c.AuthorIsOriginalParent = this.Comments.First().Author == c.Author;
				this.comments.Insert(location + 1, c);
				this.HasNewReplies = !CoreServices.Instance.SeenPosts.Contains(c.Id);
				if (c.UserIsAuthor) { this.UserParticipated = true; }
				this.ReplyCount = this.comments.Count;
			}
		}

		private Comment FindLastCommentInChain(Comment c)
		{
			var childComments = this.comments.Where(c1 => c1.ParentId == c.Id);
			if(childComments.Any())
			{
				var lastComment = childComments.OrderBy(c1 => c1.Id).LastOrDefault();
				return FindLastCommentInChain(lastComment);
			}
			else 
			{
				return c;
			}
		}

		private void CollapseIfRequired()
		{
			//if (CoreServices.Instance.CollapseList.IsOnCollapseList(this))
			//{
			//	this.IsCollapsed= true;
			//}

			//TODO: Re-Implement post collapsing
			switch (this.Category)
			{
				case PostCategory.stupid:
					this.IsCollapsed = LatestChattySettings.Instance.AutoCollapseStupid;
					break;
				case PostCategory.offtopic:
					this.IsCollapsed = LatestChattySettings.Instance.AutoCollapseOffTopic;
					break;
				case PostCategory.nws:
					this.IsCollapsed = LatestChattySettings.Instance.AutoCollapseNws;
					break;
				case PostCategory.political:
					this.IsCollapsed = LatestChattySettings.Instance.AutoCollapsePolitical;
					break;
				case PostCategory.interesting:
					this.IsCollapsed = LatestChattySettings.Instance.AutoCollapseInteresting;
					break;
				case PostCategory.informative:
					this.IsCollapsed = LatestChattySettings.Instance.AutoCollapseInformative;
					break;
				case PostCategory.newsarticle:
					this.IsCollapsed = LatestChattySettings.Instance.AutoCollapseNews;
					break;
			}
		}

	}
}