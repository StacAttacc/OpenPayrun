import { Directive, ElementRef, Input, OnDestroy, OnInit } from '@angular/core';

const FADE = '3rem';
const FADE_OVERLAY_H = '6rem';
const FADE_OVERLAY_V = '3rem';

@Directive({ selector: '[scrollFade]', standalone: true })
export class ScrollFadeDirective implements OnInit, OnDestroy {
  @Input() scrollFadeSkipUp = false;
  @Input() scrollFadeBottomColor = '';

  private el: HTMLElement;
  private ro!: ResizeObserver;
  private onScroll = () => this.update();

  private leftOverlay:   HTMLDivElement | null = null;
  private rightOverlay:  HTMLDivElement | null = null;
  private bottomOverlay: HTMLDivElement | null = null;

  constructor(ref: ElementRef<HTMLElement>) {
    this.el = ref.nativeElement;
  }

  ngOnInit() {
    if (this.scrollFadeBottomColor) {
      const parent = this.el.parentElement!;
      parent.style.position = 'relative';

      this.leftOverlay  = this.makeOverlay();
      this.rightOverlay = this.makeOverlay();
      this.bottomOverlay = this.makeOverlay();

      Object.assign(this.leftOverlay.style, {
        left: '0', bottom: '0',
        width: FADE_OVERLAY_H,
        background: `linear-gradient(to right, ${this.scrollFadeBottomColor}, transparent)`,
      });
      Object.assign(this.rightOverlay.style, {
        right: '0', bottom: '0',
        width: FADE_OVERLAY_H,
        background: `linear-gradient(to left, ${this.scrollFadeBottomColor}, transparent)`,
      });
      Object.assign(this.bottomOverlay.style, {
        left: '0', right: '0', bottom: '0',
        height: FADE_OVERLAY_V,
        background: `linear-gradient(to bottom, transparent, ${this.scrollFadeBottomColor} 60%)`,
      });

      parent.append(this.leftOverlay, this.rightOverlay, this.bottomOverlay);
    }

    this.el.addEventListener('scroll', this.onScroll, { passive: true });
    this.ro = new ResizeObserver(() => this.update());
    this.ro.observe(this.el);
    if (this.el.firstElementChild) this.ro.observe(this.el.firstElementChild);
    this.update();
  }

  ngOnDestroy() {
    this.el.removeEventListener('scroll', this.onScroll);
    this.ro.disconnect();
    this.leftOverlay?.remove();
    this.rightOverlay?.remove();
    this.bottomOverlay?.remove();
  }

  private makeOverlay(): HTMLDivElement {
    const div = document.createElement('div');
    Object.assign(div.style, {
      position: 'absolute',
      pointerEvents: 'none',
      display: 'none',
      zIndex: '2',
    });
    return div;
  }

  private update() {
    const { el } = this;
    const left  = el.scrollLeft > 0;
    const right = el.scrollLeft + el.clientWidth  < el.scrollWidth  - 1;
    const up    = !this.scrollFadeSkipUp && el.scrollTop  > 0;
    const down  = el.scrollTop  + el.clientHeight < el.scrollHeight - 1;

    if (this.scrollFadeBottomColor) {
      const thead = el.querySelector('thead');
      const theadBottom = thead ? `${thead.offsetTop + thead.offsetHeight}px` : '0px';
      this.leftOverlay!.style.top  = theadBottom;
      this.rightOverlay!.style.top = theadBottom;

      this.leftOverlay!.style.display   = left  ? '' : 'none';
      this.rightOverlay!.style.display  = right ? '' : 'none';
      this.bottomOverlay!.style.display = down  ? '' : 'none';
      el.style.maskImage = '';
      (el.style as any).webkitMaskImage = '';
      return;
    }

    // fallback: mask-image for all directions
    const masks: string[] = [];

    if (left || right) {
      const l = left  ? `transparent, black ${FADE}` : 'black, black';
      const r = right ? `black calc(100% - ${FADE}), transparent` : 'black, black';
      masks.push(`linear-gradient(to right, ${l}, ${r})`);
    }
    if (up || down) {
      const t = up   ? `transparent, black ${FADE}` : 'black, black';
      const b = down ? `black calc(100% - ${FADE}), transparent` : 'black, black';
      masks.push(`linear-gradient(to bottom, ${t}, ${b})`);
    }

    const mask = masks.join(', ');
    el.style.maskImage = mask;
    (el.style as any).webkitMaskImage = mask;
    el.style.maskComposite = masks.length > 1 ? 'intersect, intersect' : '';
    (el.style as any).webkitMaskComposite = masks.length > 1 ? 'source-in, source-in' : '';
  }
}
