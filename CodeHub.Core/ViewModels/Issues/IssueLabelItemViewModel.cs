using System;
using ReactiveUI;
using System.Drawing;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueLabelItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public Color Color { get; private set; }

        public object Image { get; private set; }

        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            internal set { this.RaiseAndSetIfChanged(ref _selected, value); }
        }

        public IReactiveCommand<object> SelectCommand { get; private set; }

        internal IssueLabelItemViewModel(IGraphicService graphicService, Octokit.Label label)
        {
            Name = label.Name;

            var color = label.Color;
            var red = color.Substring(0, 2);
            var green = color.Substring(2, 2);
            var blue = color.Substring(4, 2);

            var redB = Convert.ToByte(red, 16);
            var greenB = Convert.ToByte(green, 16);
            var blueB = Convert.ToByte(blue, 16);

            Color = Color.FromArgb(byte.MaxValue, redB, greenB, blueB);
            Image = graphicService.CreateLabelImage(Color);

            SelectCommand = ReactiveCommand.Create();
        }
    }
}

